using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AirlineManagement
{
    public static class PresentationHelper
    {
        // Keep saved original states so presentation can be reverted
        private class ControlState
        {
            public Control Control;
            public Font Font;
            public Padding Padding;
            public DockStyle Dock;
            public int RowHeight;
            public Rectangle Bounds;
        }

        private class FormState
        {
            public Font FormFont;
            public FormWindowState WindowState;
            public FormStartPosition StartPosition;
            public System.Collections.Generic.List<ControlState> Controls = new System.Collections.Generic.List<ControlState>();
        }

        private static readonly System.Collections.Generic.Dictionary<Form, FormState> _states = new System.Collections.Generic.Dictionary<Form, FormState>();

        // Reference form client size used when matching other forms
        private static Size? _referenceClientSize;
        private static Rectangle? _referenceBounds;

        // Call to apply presentation scaling. Safe to call multiple times; first call saves state.
        public static void ApplyPresentation(Form form, float baseFontSize = 16f, float controlScale = 1.15f)
        {
            if (form == null) return;

            try
            {
                if (!_states.ContainsKey(form))
                {
                    // capture original state
                    var fs = new FormState
                    {
                        FormFont = form.Font,
                        WindowState = form.WindowState,
                        StartPosition = form.StartPosition
                    };

                    foreach (Control c in form.Controls)
                    {
                        // gather control and nested children
                        foreach (Control child in GetRecursiveControls(c))
                        {
                            var cs = new ControlState
                            {
                                Control = child,
                                Font = child.Font,
                                Padding = child.Padding,
                                Dock = child.Dock,
                                Bounds = child.Bounds
                            };
                            if (child is DataGridView dgv)
                            {
                                cs.RowHeight = dgv.RowTemplate.Height;
                            }
                            fs.Controls.Add(cs);
                        }
                    }

                    _states[form] = fs;
                }

                // maximize and adjust base font
                form.StartPosition = FormStartPosition.CenterScreen;
                form.WindowState = FormWindowState.Maximized;
                try { form.Font = new Font(form.Font.FontFamily, baseFontSize, form.Font.Style); } catch { }

                // scale controls
                ScaleControls(form.Controls, controlScale);
                foreach (Control c in form.Controls)
                    ApplySpecialsRecursively(c);
            }
            catch { }
        }

        // Set the reference form used for matching sizes. Call this when Home is shown.
        public static void SetReferenceForm(Form reference)
        {
            if (reference == null) return;
            try
            {
                _referenceClientSize = reference.ClientSize;
                _referenceBounds = reference.Bounds;
            }
            catch { }
        }

        // Match a single form to the stored reference form. If no reference stored, no-op.
        public static void MatchFormToReference(Form form)
        {
            if (form == null || !_referenceClientSize.HasValue) return;
            MatchFormToReference(form, _referenceClientSize.Value, _referenceBounds);
        }

        // Match a single form to a provided reference size and bounds
        private static void MatchFormToReference(Form form, Size referenceClientSize, Rectangle? referenceBounds)
        {
            if (form == null) return;
            try
            {
                if (!_states.ContainsKey(form))
                {
                    // capture current control states
                    var fs = new FormState
                    {
                        FormFont = form.Font,
                        WindowState = form.WindowState,
                        StartPosition = form.StartPosition
                    };
                    foreach (Control c in GetRecursiveControlsForSaving(form))
                    {
                        var cs = new ControlState
                        {
                            Control = c,
                            Font = c.Font,
                            Padding = c.Padding,
                            Dock = c.Dock,
                            Bounds = c.Bounds,
                            RowHeight = (c is DataGridView dgv) ? dgv.RowTemplate.Height : 0
                        };
                        fs.Controls.Add(cs);
                    }
                    _states[form] = fs;
                }

                var saved = _states[form];

                float scaleX = (float)referenceClientSize.Width / Math.Max(1, form.ClientSize.Width);
                float scaleY = (float)referenceClientSize.Height / Math.Max(1, form.ClientSize.Height);
                float fontScale = (scaleX + scaleY) / 2f;

                // Resize form to reference size
                if (referenceBounds.HasValue)
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Size = referenceBounds.Value.Size;
                    form.Location = new Point(referenceBounds.Value.Left + (referenceBounds.Value.Width - form.Width) / 2,
                                              referenceBounds.Value.Top + (referenceBounds.Value.Height - form.Height) / 2);
                }
                else
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.ClientSize = referenceClientSize;
                }

                // scale controls based on saved bounds
                foreach (var cs in saved.Controls)
                {
                    try
                    {
                        var c = cs.Control;
                        if (c == null || c.IsDisposed) continue;
                        var b = cs.Bounds;
                        int nx = (int)Math.Round(b.X * scaleX);
                        int ny = (int)Math.Round(b.Y * scaleY);
                        int nw = Math.Max(1, (int)Math.Round(b.Width * scaleX));
                        int nh = Math.Max(1, (int)Math.Round(b.Height * scaleY));
                        c.Bounds = new Rectangle(nx, ny, nw, nh);

                        if (cs.Font != null)
                        {
                            try { c.Font = new Font(cs.Font.FontFamily, Math.Max(6f, cs.Font.Size * fontScale), cs.Font.Style); } catch { }
                        }

                        if (c is DataGridView dgv)
                        {
                            try { dgv.RowTemplate.Height = Math.Max(16, (int)(cs.RowHeight * fontScale)); } catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        // Match all currently open forms to the stored reference form
        public static void MatchAllOpenFormsToReference()
        {
            if (!_referenceClientSize.HasValue) return;
            var forms = Application.OpenForms.Cast<Form>().ToArray();
            foreach (var f in forms)
            {
                try { if (f != null) MatchFormToReference(f); } catch { }
            }
        }

        private static System.Collections.Generic.IEnumerable<Control> GetRecursiveControls(Control root)
        {
            var list = new System.Collections.Generic.List<Control>();
            if (root == null) return list;
            list.Add(root);
            if (!root.HasChildren) return list;
            foreach (Control c in root.Controls)
                list.AddRange(GetRecursiveControls(c));
            return list;
        }

        private static System.Collections.Generic.IEnumerable<Control> GetRecursiveControlsForSaving(Control root)
        {
            var list = new System.Collections.Generic.List<Control>();
            if (root == null) return list;
            foreach (Control c in root.Controls)
            {
                list.Add(c);
                if (c.HasChildren)
                    list.AddRange(GetRecursiveControlsForSaving(c));
            }
            return list;
        }

        // Revert presentation changes saved earlier
        public static void RevertPresentation(Form form)
        {
            if (form == null) return;
            try
            {
                if (!_states.ContainsKey(form)) return;
                var fs = _states[form];
                try { form.Font = fs.FormFont; } catch { }
                try { form.WindowState = fs.WindowState; } catch { }
                try { form.StartPosition = fs.StartPosition; } catch { }

                foreach (var cs in fs.Controls)
                {
                    try { cs.Control.Font = cs.Font; } catch { }
                    try { cs.Control.Padding = cs.Padding; } catch { }
                    try { cs.Control.Dock = cs.Dock; } catch { }
                    try
                    {
                        if (cs.Control is DataGridView dgv)
                            dgv.RowTemplate.Height = cs.RowHeight;
                    }
                    catch { }
                }

                _states.Remove(form);
            }
            catch { }
        }

        private static void ScaleControls(Control.ControlCollection controls, float scale)
        {
            foreach (Control c in controls)
            {
                try
                {
                    if (c.Font != null)
                        c.Font = new Font(c.Font.FontFamily, Math.Max(8f, c.Font.Size * scale), c.Font.Style);

                    // enlarge padding/margins for readability
                    c.Padding = new Padding(
                        (int)(c.Padding.Left * scale),
                        (int)(c.Padding.Top * scale),
                        (int)(c.Padding.Right * scale),
                        (int)(c.Padding.Bottom * scale)
                    );
                }
                catch { }

                if (c.HasChildren)
                    ScaleControls(c.Controls, scale);
            }
        }

        private static void ApplySpecialsRecursively(Control control)
        {
            if (control is DataGridView dgv)
            {
                try
                {
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.RowTemplate.Height = Math.Max(28, dgv.RowTemplate.Height * 2);
                    dgv.Font = new Font(dgv.Font.FontFamily, Math.Max(10f, dgv.Font.Size + 2f));
                    dgv.Dock = DockStyle.Fill;
                }
                catch { }
            }

            if (control is Button btn)
            {
                try { btn.Padding = new Padding(12); } catch { }
            }

            if (control is PictureBox pb)
            {
                try { pb.SizeMode = PictureBoxSizeMode.Zoom; } catch { }
            }

            // recurse
            if (control.HasChildren)
            {
                foreach (Control child in control.Controls)
                    ApplySpecialsRecursively(child);
            }
        }
    }
}

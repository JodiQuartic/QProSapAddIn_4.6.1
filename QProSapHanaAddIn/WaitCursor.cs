using System;
using System.Windows.Input;

namespace QProSapAddIn
{
    public class WaitCursor : IDisposable
    {
        private System.Windows.Input.Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }
}
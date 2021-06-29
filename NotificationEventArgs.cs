using System;
namespace Notification
{
    public class TestEventArgs : EventArgs
    {
        public readonly string test;
        public TestEventArgs(string test)
        {
            this.test = test;
        }
    }
}
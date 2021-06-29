using System.Diagnostics;
using Notification;
using System;

namespace CSharpObserver
{
    class TestA
    {
        public void AddObservers()
        {
            NotificationCenter.AddObserver(SayHi, NotificationMessage.HELLO_WORLD);
        }

        public void RemoveObservers()
        {
            NotificationCenter.RemoveObserver(SayHi, NotificationMessage.HELLO_WORLD);
        }

        public void SayHi(EventArgs eventArgs = null)
        {
            TestEventArgs args = (TestEventArgs)eventArgs;
            Console.WriteLine("Hello World {0}", args.test);
        }
    }
}
using System;
using Notification;
namespace CSharpObserver
{
    class Program
    {
        static void Main(string[] args)
        {
            TestA a = new TestA();
            a.AddObservers();

            NotificationCenter.SendNotification(NotificationMessage.HELLO_WORLD, new TestEventArgs("Hello"));

            a.RemoveObservers();
        }
    }
}

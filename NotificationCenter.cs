using System;
using System.Collections.Generic;
namespace Notification
{
    public delegate void NotifyEventHandler(EventArgs eventArgs = null);

    class MessageInfo
    {
        // 事件封装委托，这样delegate除了+=和-=以外就不会暴露出去，即便声明为public
        // public event NotifyEventHandler selector;
        // 事件跟private NotifyEventHandler selector的区别就是事件可以在类外调用+=和-=
        private NotifyEventHandler selector;
        private List<bool> wait_end;

        /// <summary>
        /// 添加委托
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="wait_end"></param>
        public void AddSelector(NotifyEventHandler selector, bool wait_end)
        {
            if (this.selector == null)
            {
                this.selector = selector;
            }
            else
            {
                this.selector += selector;
            }

            if (this.wait_end == null)
            {
                this.wait_end = new List<bool>();
            }

            this.wait_end.Add(wait_end);
        }

        /// <summary>
        /// 移除委托
        /// </summary>
        /// <param name="selector"></param>
        public void RemoveSelctor(NotifyEventHandler selector)
        {
            if (this.selector != null)
            {
                Delegate[] delegates = this.selector.GetInvocationList();

                for (int i = 0; i < delegates.Length; i++)
                {
                    if (delegates[i].Equals(selector))
                    {
                        this.wait_end.RemoveAt(i);
                    }
                }

                this.selector -= selector;
            }
        }

        /// <summary>
        /// 执行委托
        /// </summary>
        /// <param name="args">EventArgs类型的参数</param>
        public bool PerformSelector(EventArgs args = null)
        {
            bool wait_end = true;
            if (this.selector != null)
            {
                this.selector(args);
                for (int i = 0; i < this.wait_end.Count; i++)
                {
                    if (!this.wait_end[i])
                    {
                        wait_end = false;
                    }
                }
            }
            return wait_end;
        }
    }

    /// <summary>
    /// 消息链表节点
    /// </summary>
    class NotificationTail
    {
        public NotificationTail next;
        public NotificationMessage message;
        public EventArgs context;

        public NotificationTail(NotificationTail next, NotificationMessage message, EventArgs context)
        {
            this.next = next;
            this.message = message;
            this.context = context;
        }
    }

    class NotificationCenter
    {
        private static Dictionary<NotificationMessage, MessageInfo> observers = new Dictionary<NotificationMessage, MessageInfo>();
        private static bool is_block_notifications = false;
        private static NotificationTail pending_notification_head = null;
        private static NotificationTail pending_notification_tail = null;

        public static void AddObserver(NotifyEventHandler selector, NotificationMessage message, bool wait_end = false)
        {
            if (!NotificationCenter.observers.ContainsKey(message))
            {
                NotificationCenter.observers[message] = new MessageInfo();
            }
            NotificationCenter.observers[message].AddSelector(selector, wait_end);
        }

        public static bool RemoveObserver(NotifyEventHandler selector, NotificationMessage message)
        {
            if (NotificationCenter.observers.ContainsKey(message))
            {
                NotificationCenter.observers[message].RemoveSelctor(selector);
                return true;
            }

            return false;
        }
        /// <summary>
        /// 不会被block的发出消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void SendNotification(NotificationMessage message, EventArgs args = null)
        {
            if (NotificationCenter.observers.ContainsKey(message))
            {
                NotificationCenter.observers[message].PerformSelector(args);
            }
        }

        /// <summary>
        /// 会被block的发出消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        public static void PostNotification(NotificationMessage message, EventArgs args = null)
        {
            if (NotificationCenter.is_block_notifications)
            {
                NotificationTail tail = new NotificationTail(null, message, args);
                if (NotificationCenter.pending_notification_tail != null)
                {
                    NotificationCenter.pending_notification_tail.next = tail;
                }
                NotificationCenter.pending_notification_tail = tail;
                if (NotificationCenter.pending_notification_head == null)
                {
                    NotificationCenter.pending_notification_head = tail;
                }
            }
            else
            {
                NotificationCenter.SendNotification(message, args);
            }
        }

        public static bool IsBlockNotifications()
        {
            return NotificationCenter.is_block_notifications;
        }

        public static bool BlockNotifications()
        {
            return NotificationCenter.is_block_notifications = true;
        }

        public static void UnBlockNotifications()
        {
            NotificationTail trail = NotificationCenter.pending_notification_head;
            while (trail != null)
            {
                NotificationMessage message = trail.message;
                if (NotificationCenter.observers.ContainsKey(message))
                {
                    MessageInfo observers = NotificationCenter.observers[message];
                    NotificationCenter.pending_notification_head = trail.next;
                    bool unblocking_flag = observers.PerformSelector(trail.context);
                    if (unblocking_flag)
                    {
                        return;
                    }
                }
                trail = NotificationCenter.pending_notification_head;
            }
            NotificationCenter.PurgePendingNotifications();
        }

        private static void PurgePendingNotifications()
        {
            NotificationCenter.is_block_notifications = false;
            NotificationCenter.pending_notification_head = null;
            NotificationCenter.pending_notification_tail = null;
        }
    }
}
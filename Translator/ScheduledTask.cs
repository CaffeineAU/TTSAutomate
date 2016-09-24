using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTSAutomate
{
    public class ScheduledTask
    {
        internal readonly Action Action;
        internal System.Timers.Timer Timer;
        internal EventHandler TaskComplete;

        public ScheduledTask(Action action, int timeoutMs)
        {
            Action = action;
            Timer = new System.Timers.Timer() { Interval = timeoutMs };
            Timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer.Stop();
            Timer.Elapsed -= TimerElapsed;
            Timer = null;

            Action();
            TaskComplete(this, EventArgs.Empty);
        }
    }

    public class Scheduler
    {
        private readonly ConcurrentDictionary<Action, ScheduledTask> _scheduledTasks = new ConcurrentDictionary<Action, ScheduledTask>();

        public void Execute(Action action, int timeoutMs)
        {
            var task = new ScheduledTask(action, timeoutMs);
            task.TaskComplete += RemoveTask;
            _scheduledTasks.TryAdd(action, task);
            task.Timer.Start();
        }

        private void RemoveTask(object sender, EventArgs e)
        {
            var task = (ScheduledTask)sender;
            task.TaskComplete -= RemoveTask;
            ScheduledTask deleted;
            _scheduledTasks.TryRemove(task.Action, out deleted);
        }
    }
}

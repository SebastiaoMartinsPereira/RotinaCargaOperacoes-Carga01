using System;
using System.Threading;

namespace Core
{
    public class Spinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        private int counter = 0;
        private int left;
        private int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public static string Sequence1 => Sequence;

        public int Counter { get => counter; set => counter = value; }
        public int Left { get => left; set => left = value; }
        public int Top { get => top; set => top = value; }

        public int Delay => delay;

        public bool Active { get => active; set => active = value; }

        public Thread Thread => thread;


        public Spinner(int left, int top, int delay = 100)
        {
            this.Left = left;
            this.Top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }


        public void Start()
        {
            Active = true;
            if (!Thread.IsAlive)
                Thread.Start();
        }

        public void Stop()
        {
            Active = false;
            Draw(' ');
        }

 
        public void SetLeft(int left)
        {
            this.Left = left;
        }

        public void SetTop(int top)
        {
            this.Top = top;
        }


        public void SetCursorPosition(int left, int top)
        {
            SetLeft(left);
            SetTop(top);
        }

        private void Spin()
        {
            while (Active)
            {
                Turn();
                Thread.Sleep(Delay);
            }
        }

        private void Draw(char c)
        {
            Console.SetCursorPosition(Left, Top);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(c);
        }

        private void Turn()
        {
            Draw(Sequence1[++Counter % Sequence1.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }

}

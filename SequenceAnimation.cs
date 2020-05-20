using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

namespace VichMat.Solution
{
    class SequenceAnimation
    {
        readonly struct Complex
        {
            public Complex(UIElement elem, AnimationTimeline anim, DependencyProperty prop)
            {
                _element = elem;
                _animation = anim;
                _property = prop;
            }

            private readonly UIElement _element;
            private readonly AnimationTimeline _animation;
            private readonly DependencyProperty _property;

            public void BeginAnimation()
            {
                _element.BeginAnimation(_property, _animation);
            }

            public void Completed(EventHandler completedEvent)
            {
                _animation.Completed += completedEvent;
            }

        }
        private readonly Stack<Complex> _stack;

        public SequenceAnimation()
        {
            _stack = new Stack<Complex>();
        }

        public bool AddAnimation(UIElement element, AnimationTimeline anim, DependencyProperty prop)
        {
            if (anim != null && prop != null && element != null)
            {
                Complex complex = new Complex(element, anim, prop);
                _stack.Push(complex);
                return true;
            }
            return false;
        }

        public void Play(bool together = false)
        {
            if (_stack.Count != 0)
            {
                if (together == false)
                {
                    var complex = _stack.Pop();
                    complex.Completed(NextPlay);
                    complex.BeginAnimation();
                }
                else
                {
                    var array = _stack.ToArray();

                    foreach (var complex in array)
                    {
                        complex.BeginAnimation();
                    }
                }
            }
        }

        private void NextPlay(object sender, EventArgs e)
        {
            var send = (AnimationClock)sender;
            Play();
        }
    }
}

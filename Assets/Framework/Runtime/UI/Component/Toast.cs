using System;
using System.Collections;
using System.IO;
using Framework.Context;
using Framework.UI.Core;
using UnityEngine;

namespace Framework.Runtime.UI.Component
{
    public class Toast
    {
        private const string DEFAULT_VIEW_LOCATOR_KEY = "_DEFAULT_VIEW_LOCATOR";
        private const string DEFAULT_VIEW_NAME = "UI/Toast";

        private static string viewName;
        public static string ViewName
        {
            get { return string.IsNullOrEmpty(viewName) ? DEFAULT_VIEW_NAME : viewName; }
            set { viewName = value; }
        }

        private static IViewLocator GetUIViewLocator()
        {
            ApplicationContext context = new ApplicationContext();
            IViewLocator locator = context.GetService<IViewLocator>();
            return locator;
        }

        public static Toast Show(string text, float duration = 3f, Action callback = null)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ViewName;

            IViewLocator locator = GetUIViewLocator();
            ToastView view = locator.LoadView<ToastView>(viewName);
            if (view == null)
                throw new FileNotFoundException("Not found the \"ToastView\".");

            Toast toast = new Toast(view, text, duration);
            toast.Show();
            return toast;
        }

        private readonly float duration;
        private readonly string text;
        private readonly ToastView view;
        private readonly Action callback;
        
        protected Toast(ToastView view, string text, float duration = 3, Action callback = null)
        {
            this.view = view;
            this.text = text;
            this.duration = duration;
            this.callback = callback;
        }

        public float Duration
        {
            get { return this.duration; }
        }

        public string Text
        {
            get { return this.text; }
        }

        public ToastView View
        {
            get { return this.view; }
        }

        public void Cancel()
        {
            if (this.view == null)
                return;

            if (!this.view.Visible)
            {
                view.Destroy();
                return;
            }

            if (this.view.ExitAnimation != null)
            {
                this.view.ExitAnimation.OnEnd(() =>
                {
                    view.Destroy();
                    this.DoCallback();
                }).Play();
            }
            else
            {
                view.Destroy();
                this.DoCallback();
            }
        }

        public void Show()
        {
            if (this.view.Visible)
                return;

            this.view.Show();
            this.view.Text.text = this.text;
            view.EnterAnimation?.Play();

            this.view.StartCoroutine(DelayDismiss(duration));
        }

        protected IEnumerator DelayDismiss(float duration)
        {
            yield return new WaitForSeconds(duration);
            this.Cancel();
        }

        protected void DoCallback()
        {
            try
            {
                if (this.callback != null)
                    this.callback();
            }
            catch (Exception)
            {
            }
        }
    }
}
using LOIN.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LOIN.Viewer.Views
{
    public abstract class ContextView<T>: INotifyPropertyChanged where T: IContextEntity
    {
        private readonly ContextSelector selector;

        public ContextView(T context, ContextSelector selector)
        {
            Context = context;
            this.selector = selector;
        }

        private bool _isSelected;


        public virtual bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (value)
                    selector.Add(Context);
                else
                    selector.Remove(Context);
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public T Context { get; }

        public string Name => Context.Name;
        public string Description => Context.Description;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

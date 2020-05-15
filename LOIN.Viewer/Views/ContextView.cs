using LOIN.Context;
using System.ComponentModel;

namespace LOIN.Viewer.Views
{
    public abstract class ContextView<T>: INotifyPropertyChanged where T: IContextEntity
    {
        private readonly ContextSelector selector;
        private string lang;

        protected ContextView(T context, ContextSelector selector)
        {
            Context = context;
            this.selector = selector;

            Language.PropertyChanged += (_, p) => {
                if (p.PropertyName != nameof(Language.Lang))
                    return;
                lang = Language.Lang;
            };
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

        public string Name2 => Context.GetName(lang) ?? Name;
        public string Description2 => Context.GetDescription(lang) ?? Description;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using LOIN.Context;
using System.ComponentModel;

namespace LOIN.Viewer.Views
{
    public abstract class ContextView : INotifyPropertyChanged
    {
        protected readonly ContextSelector selector;
        protected string lang;

        private bool _isSelected;

        public virtual bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                if (value)
                    selector.Add(this);
                else
                    selector.Remove(Entity);
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        internal void Unselect()
        {
            _isSelected = false;
            OnPropertyChanged(nameof(IsSelected));
        }

        public IContextEntity Entity { get; }

        public string Id => Entity.Id;

        protected ContextView(IContextEntity entity, ContextSelector selector)
        {
            this.selector = selector;
            Entity = entity;

            lang = Language.Lang;
            Language.PropertyChanged += (_, p) =>
            {
                if (p.PropertyName != nameof(Language.Lang))
                    return;
                lang = Language.Lang;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public abstract class ContextView<T> : ContextView where T : IContextEntity
    {

        protected ContextView(T context, ContextSelector selector) : base(context, selector)
        {
            Context = context;
        }



        public T Context { get; }

        public string Name => Context.Name;
        public string Description => Context.Description;

        public string Name2 => Context.GetName(lang) ?? Name;
        public string Description2 => Context.GetDescription(lang) ?? Description;


    }
}

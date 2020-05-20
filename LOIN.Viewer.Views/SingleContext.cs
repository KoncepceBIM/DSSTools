using System;
using System.ComponentModel;
using System.Linq;

namespace LOIN.Viewer.Views
{
    public class SingleContext: INotifyPropertyChanged
    {
        private readonly ContextSelector selector;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        // Declare the delegate (if using non-generic pattern).
        public delegate void ContextUpdatedEventHandler(object sender, EventArgs e);

        // Declare the event.
        public event ContextUpdatedEventHandler ContextUpdatedEvent;

        public ActorView Actor { get; private set; }
        public BreakdownItemView BreakdownItem { get; private set; }
        public MilestoneView Milestone { get; private set; }
        public ReasonView Reason { get; private set; }

        public SingleContext(ContextSelector selector)
        {
            this.selector = selector;
            this.selector.ContextUpdatedEvent += (s, a) =>
            {
                Update();
            };
            Update();
        }

        private void Update()
        { 
            Actor = selector.ContextViews.OfType<ActorView>().FirstOrDefault();
            BreakdownItem = selector.ContextViews.OfType<BreakdownItemView>().FirstOrDefault();
            Milestone = selector.ContextViews.OfType<MilestoneView>().FirstOrDefault();
            Reason = selector.ContextViews.OfType<ReasonView>().FirstOrDefault();

            OnPropertyChanged(nameof(Actor));
            OnPropertyChanged(nameof(BreakdownItem));
            OnPropertyChanged(nameof(Milestone));
            OnPropertyChanged(nameof(Reason));
            OnPropertyChanged(nameof(IsComplete));

            ContextUpdatedEvent?.Invoke(this, EventArgs.Empty);
        }

        public bool IsComplete => 
            Actor != null && 
            BreakdownItem != null && 
            Milestone != null && 
            Reason != null;
    }
}

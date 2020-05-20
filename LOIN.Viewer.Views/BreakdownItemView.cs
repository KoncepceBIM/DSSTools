using LOIN.Context;
using System.Collections.Generic;
using System.Linq;

namespace LOIN.Viewer.Views
{
    public class BreakdownItemView: ContextView<BreakdownItem>
    {
        public BreakdownItemView(BreakdownItem item, BreakdownItemView parent, ContextSelector selector, bool cascadeSelect): base(item, selector)
        {
            Children = item.Children.Select(i => new BreakdownItemView(i, this, selector, cascadeSelect)).ToList();
            Parent = parent;
            CascadeSelect = cascadeSelect;
        }

        public string Code => Context.Code;

        public IEnumerable<BreakdownItemView> Children { get; }

        public IEnumerable<BreakdownItemView> GetDeepSelected()
        {
            if (IsSelected)
                yield return this;
            foreach (var item in Children.SelectMany(c => c.GetDeepSelected()))
                yield return item;
        }

        public BreakdownItemView GetDeep(string id)
        {
            if (Entity.Id == id) return this;
            return Children.Select(c => c.GetDeep(id)).Where(c => c != null).FirstOrDefault();
        }

        public override bool IsSelected { 
            get => base.IsSelected; 
            set 
            {
                base.IsSelected = value;
                // cascade select
                if (CascadeSelect && Children != null)
                    foreach (var c in Children)
                        c.IsSelected = value;
            }
        }

        private bool _isExpanded;
        public bool IsExpanded  
        {  
            get => _isExpanded; 
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); } 
        }

        public string ShowName
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Code))
                    parts.Add(Code);
                if (!string.IsNullOrWhiteSpace(Name))
                    parts.Add(Name);
                if (!string.IsNullOrWhiteSpace(Description))
                    parts.Add(Description);

                if (parts.Count == 0)
                    parts.Add("-none-");

                return string.Join(":", parts);
            }
        }

        public BreakdownItemView Parent { get; }
        public bool CascadeSelect { get; }
    }
}

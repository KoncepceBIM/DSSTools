using LOIN.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOIN.Viewer.Views
{
    public class BreakedownItemView: ContextView<BreakedownItem>
    {
        public BreakedownItemView(BreakedownItem item, ContextSelector selector): base(item, selector)
        {
            Children = item.Children.Select(i => new BreakedownItemView(i, selector)).ToList();
        }

        public string Code => Context.Code;

        public IEnumerable<BreakedownItemView> Children { get; }

        public override bool IsSelected { 
            get => base.IsSelected; 
            set 
            {
                base.IsSelected = value;
                // cascade select
                if (Children != null)
                    foreach (var c in Children)
                        c.IsSelected = value;
            }
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

                return string.Join(':', parts);
            }
        }
    }
}

using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rentrey.Maui
{
    public partial class PropertyPage : ContentPage, IQueryAttributable
    {
        private Property _property;

        public Property Property
        {
            get => _property;
            set
            {
                _property = value;
                OnPropertyChanged(nameof(Property));
            }
        }

        public PropertyPage()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("property") && query["property"] is Property receivedProperty)
            {
                this.Property = receivedProperty;
                this.BindingContext = this.Property;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

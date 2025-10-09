using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Rentrey.Maui;

// Simple Data Models for the Payment Page
public class PaymentRecord
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public bool IsSuccessful { get; set; }
    public Color StatusColor => IsSuccessful ? Color.FromArgb("#4CAF50") : Color.FromArgb("#EF5350");
}

public partial class PaymentPage : ContentPage, INotifyPropertyChanged
{
    private decimal _currentBalance = 1500.00m;
    public decimal CurrentBalance
    {
        get => _currentBalance;
        set
        {
            if (_currentBalance != value)
            {
                _currentBalance = value;
                OnPropertyChanged(nameof(CurrentBalance));
            }
        }
    }

    public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
    public ObservableCollection<PaymentRecord> PaymentHistory { get; set; }
    public ICommand MakePaymentCommand { get; }

    public PaymentPage()
    {
        InitializeComponent();

        PaymentHistory = new ObservableCollection<PaymentRecord>
        {
            new PaymentRecord { Amount = 1500.00m, Date = DateTime.Now.AddMonths(-1), Description = "Monthly Rent Payment", IsSuccessful = true },
            new PaymentRecord { Amount = 50.00m, Date = DateTime.Now.AddDays(-15), Description = "Maintenance Fee", IsSuccessful = true },
            new PaymentRecord { Amount = 1500.00m, Date = DateTime.Now.AddMonths(-2), Description = "Monthly Rent Payment", IsSuccessful = false }
        };

        MakePaymentCommand = new Command(OnMakePaymentClicked);

        BindingContext = this;
    }

    private void OnMakePaymentClicked()
    {
        // Placeholder logic for making a payment
        DisplayAlert("Payment", "Navigating to payment portal...", "OK");
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
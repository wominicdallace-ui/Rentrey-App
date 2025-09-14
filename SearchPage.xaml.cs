<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Rentrey.Maui"
             x:Class="Rentrey.Maui.SearchPage"
             BackgroundColor="#F5F5F5"
             Title="Search">

    <Grid RowDefinitions="Auto, *, Auto">

        <Frame Grid.Row="0" CornerRadius="10" Padding="5" Margin="15,15,15,5">
            <HorizontalStackLayout Spacing="10" VerticalOptions="Center">
                <Entry Placeholder="Search for an address or suburb" 
                       HorizontalOptions="FillAndExpand" 
                       VerticalOptions="Center" 
                       FontSize="16" 
                       Margin="5,0"/>
                <Image Source="search_icon.svg" HeightRequest="25" VerticalOptions="Center"/>
            </HorizontalStackLayout>
        </Frame>

        <VerticalStackLayout Grid.Row="1" Spacing="10">
            <Label Text="Find a Property on the Map" FontSize="20" FontAttributes="Bold" TextColor="#333333" Margin="15,10,15,0"/>
            <Frame CornerRadius="10" Padding="0" Margin="15,0,15,10" HeightRequest="250">
                <Image Source="placeholdermap.png" Aspect="AspectFill" />
            </Frame>
        </VerticalStackLayout>

        <VerticalStackLayout Grid.Row="2" Spacing="10">
            <Label Text="Newly Added Properties" FontSize="20" FontAttributes="Bold" TextColor="#333333" Margin="15,0"/>
            <CollectionView ItemsSource="{Binding NewlyAddedProperties}" HeightRequest="250">
                <CollectionView.ItemsLayout>
                    <LinearItemsLayout Orientation="Horizontal" ItemSpacing="15"/>
                </CollectionView.ItemsLayout>
                <CollectionView.ItemTemplate>
                    <DataTemplate>
                        <Frame CornerRadius="10" Padding="0" WidthRequest="250">
                            <Frame.GestureRecognizers>
                                <TapGestureRecognizer Command="{Binding Source={RelativeSource AncestorType={x:Type local:SearchPage}}, Path=BindingContext.NavigateToPropertyCommand}"
                                                      CommandParameter="{Binding .}" />
                            </Frame.GestureRecognizers>
                            <VerticalStackLayout>
                                <Image Source="{Binding ImageSource}" Aspect="AspectFill" HeightRequest="180" />
                                <VerticalStackLayout Padding="10">
                                    <Label Text="{Binding Details}" HorizontalOptions="Start" FontSize="14" FontAttributes="Bold"/>
                                    <Label Text="{Binding Address}" HorizontalOptions="Start" FontSize="16" FontAttributes="Bold" Margin="0,5,0,0" />
                                </VerticalStackLayout>
                            </VerticalStackLayout>
                        </Frame>
                    </DataTemplate>
                </CollectionView.ItemTemplate>
            </CollectionView>
        </VerticalStackLayout>
    </Grid>
</ContentPage>

set V=2.10.4
nuget push VirtoCommerce.PricingModule.Client.%V%.nupkg -Source nuget.org -ApiKey %1
nuget push VirtoCommerce.PricingModule.Data.%V%.nupkg -Source nuget.org -ApiKey %1
pause

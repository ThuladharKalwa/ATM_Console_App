﻿using ATM.Models;
using ATM.Services.DBModels;
using ATM.Services.Exceptions;
using ATM.Services.IServices;
using System.Linq;

namespace ATM.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMapperService _mapperService;
        private readonly BankContext _bankContext;

        public CurrencyService(BankContext bankContext, IMapperService mapperService)
        {
            _mapperService = mapperService;
            _bankContext = bankContext;
        }

        public void CheckCurrencyExistance(string bankId, string currencyName)
        {
            if (!_bankContext.Currency.Any(c => c.BankId == bankId && c.Name == currencyName))
            {
                throw new CurrencyDoesNotExistException();
            }
        }

        public void ValidateCurrencyName(string bankId, string currencyName)
        {
            if (_bankContext.Currency.Any(c => c.BankId == bankId && c.Name == currencyName))
            {
                throw new CurrencyAlreadyExistsException();
            }
        }

        public Currency GetCurrencyByName(string bankId, string currencyName)
        {
            CheckCurrencyExistance(bankId, currencyName);
            CurrencyDBModel currencyRecord = _bankContext.Currency.FirstOrDefault(c => c.BankId == bankId && c.Name == currencyName);
            return _mapperService.MapDBToCurrency(currencyRecord);
        }

        public Currency CreateCurrency(string currencyName, double exchangeRate)
        {
            Currency newCurrency = new Currency
            {
                Name = currencyName,
                ExchangeRate = exchangeRate,
            };
            return newCurrency;
        }

        public void AddCurrency(string bankId, Currency currency)
        {
            currency.BankId = bankId;
            CurrencyDBModel currencyRecord = _mapperService.MapCurrencyToDB(currency);
            _bankContext.Currency.Add(currencyRecord);
            _bankContext.SaveChanges();
        }

        public void UpdateCurrency(string bankId, string currencyName, Currency updateCurrency)
        {
            CurrencyDBModel currentCurrencyRecord = _bankContext.Currency.First(c => c.BankId == bankId && c.Name == currencyName);
            currentCurrencyRecord.ExchangeRate = updateCurrency.ExchangeRate;
            _bankContext.SaveChanges();
        }

        public void DeleteCurrency(string bankId, string currencyName)
        {
            CheckCurrencyExistance(bankId, currencyName);
            _bankContext.Remove(_bankContext.Currency.First(c => c.BankId == bankId && c.Name == currencyName));
            _bankContext.SaveChanges();
        }
    }
}

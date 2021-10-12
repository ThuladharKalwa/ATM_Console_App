﻿using ATM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATM.Services
{
    public class BankManager
    {
        private List<Bank> banks;
        private TransactionHandler transactionHandler;
        private EncryptionService encryptionService;
        private DataHandler dataHandler;

        public BankManager()
        {
            transactionHandler = new TransactionHandler();
            encryptionService = new EncryptionService();
            dataHandler = new DataHandler();
            this.banks = dataHandler.ReadBankData();
            if (this.banks == null)
            {
                this.banks = new List<Bank>();
            }
        }

        public Dictionary<string, string> GetBankNames()
        {
            Dictionary<string, string> bankNames = new Dictionary<string, string>();
            foreach(Bank bank in this.banks)
            {
                bankNames.Add(bank.Id, bank.Name);
            }
            return bankNames;
        }

        public string CreateNewBank(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new BankCreationFailedException();
            }
            if (this.banks.FirstOrDefault(b => b.Name == name) != null)
            {
                throw new BankNameAlreadyExistsException();
            }
            Bank newBank = new Bank
            {
                Name = name,
                Id = Guid.NewGuid().ToString("N"),
                Accounts = new List<Account>()
            };
            this.banks.Add(newBank);
            dataHandler.WriteBankData(this.banks);
            return newBank.Id;
        }

        public void CreateNewAccount(string bankId, string name, string pin, string username, AccountType accountType)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.FirstOrDefault(a => a.Username == username);
            if (account != null)
            {
                throw new UsernameAlreadyExistsException();
            }
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(pin) || accountType == (AccountType)0)
            {
                throw new AccountCreationFailedException();
            }
            else
            {
                Account newAccount = new Account
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Username = username,
                    Name = name,
                    AccountType = accountType,
                    Pin = encryptionService.ComputeSha256Hash(pin),
                    Balance = 1500,
                    Transactions = new List<Transaction>()
                };
                newAccount.Transactions.Add(transactionHandler.NewTransaction(1500, (TransactionType)1));
                bank.Accounts.Add(newAccount);
                dataHandler.WriteBankData(this.banks);
            }
        }

        public void Deposit(string bankId, string accountId, decimal amount)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.Find(a => a.Id == accountId);
            if (amount <= 0)
            {
                throw new InvalidAmountException();
            }
            else
            {
                account.Balance += amount;
                account.Transactions.Add(transactionHandler.NewTransaction(amount, (TransactionType)3));
                dataHandler.WriteBankData(this.banks);
            }
        }


        public void Withdraw(string bankId, string accountId, decimal amount)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.Find(a => a.Id == accountId);
            if (amount <= 0 || amount > account.Balance)
            {
                throw new InvalidAmountException();
            }
            else
            {
                account.Balance -= amount;
                account.Transactions.Add(transactionHandler.NewTransaction(amount, (TransactionType)2));
                dataHandler.WriteBankData(this.banks);
            }
        }


        public void Transfer(string selectedBankId, string selectedAccountId, string transferToBankId, string transferToAccountId, decimal amount)
        {
            Bank bank = this.banks.Find(b => b.Id == selectedBankId);
            Bank toBank = this.banks.Find(b => b.Id == selectedBankId);
            Account account = bank.Accounts.Find(a => a.Id == selectedAccountId);
            Account transferToAccount = toBank.Accounts.Find(a => a.Id == transferToAccountId);
            if (amount <= 0 || amount > account.Balance)
            {
                throw new InvalidAmountException();
                throw new TransferFailedException();
            }
            else
            {
                if (transferToAccount == null)
                {
                    throw new TransferFailedException();
                }
                else
                {
                    account.Balance -= amount;
                    account.Transactions.Add(transactionHandler.NewTransaction(amount, (TransactionType)2));
                    transferToAccount.Balance += amount;
                    transferToAccount.Transactions.Add(transactionHandler.NewTransaction(amount, (TransactionType)3));
                    dataHandler.WriteBankData(this.banks);
                }
            }

        }

        public List<Transaction> GetTransactions(string bankId, string accountId)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.Find(a => a.Id == accountId);
            return account.Transactions;
        }

        public decimal GetBalance(string bankId, string accountId)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.Find(a => a.Id == accountId);
            return account.Balance;
        }

        public string CheckAccountExistance(string bankId, string username)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.FirstOrDefault(a => a.Username == username);
            if (account == null)
            {
                throw new UserNotFoundException();
            }
            return account.Id;
        }

        public bool CheckBankExistance(string bankId)
        {
            Bank bank = this.banks.FirstOrDefault(b => b.Id == bankId);
            if (bank == null)
            {
                throw new BankDoesnotExistException();
            }
            return true;
        }

        public void Authenticate(string bankId, string accountId, string userInput)
        {
            Bank bank = this.banks.Find(b => b.Id == bankId);
            Account account = bank.Accounts.Find(a => a.Id == accountId);
            string hashedUserInput = encryptionService.ComputeSha256Hash(userInput);
            if (hashedUserInput != account.Pin)
            {
                throw new AuthenticationFailedException();
            }
        }
    }
}

﻿using ATM.Models;
using ATM.Models.Enums;
using System;
using AutoMapper;
using ATM.Services.Exceptions;
using ATM.Services.DBModels;
using System.Collections.Generic;
using System.Linq;

namespace ATM.Services
{
    public class TransactionService
    {
        private readonly IDGenService idGenService;
        private readonly MapperConfiguration transactionDBConfig;
        private readonly Mapper transactionDBMapper;

        public TransactionService()
        {
            idGenService = new IDGenService();
            transactionDBConfig = new MapperConfiguration(cfg => cfg.CreateMap<Transaction, TransactionDBModel>());
            transactionDBMapper = new Mapper(transactionDBConfig);
        }

        public Transaction CreateTransaction(string bankId, string accountId, decimal amount, TransactionType transactionType, TransactionNarrative transactionNarrative, string fromAccId, string toBankId = null, string toAccId = null)
        {
            Transaction newTransaction = new Transaction
            {
                Id = idGenService.GenTransactionId(bankId, accountId),
                TransactionDate = DateTime.Now,
                TransactionType = transactionType,
                BankId = bankId,
                AccountId = fromAccId,
                ToBankId = toBankId,
                ToAccountId = toAccId,
                TransactionNarrative = transactionNarrative,
                TransactionAmount = amount
            };
            return newTransaction;
        }

        public void AddTransaction(string bankId, string accountId, Transaction transaction)
        {
            transaction.AccountId = accountId;
            transaction.BankId = bankId;
            TransactionDBModel transactionRecord = transactionDBMapper.Map<TransactionDBModel>(transaction);
            using (BankContext bankContext = new BankContext())
            {
                bankContext.Transaction.Add(transactionRecord);
                bankContext.SaveChanges();
            }
        }

        public Transaction GetTransactionById(string bankId, string txnId)
        {
            using (BankContext bankContext = new BankContext())
            {
                TransactionDBModel transactionRecord = bankContext.Transaction.FirstOrDefault(t => t.BankId == bankId && t.Id == txnId);
                if (transactionRecord == null)
                {
                    throw new TransactionNotFoundException();
                }
                return transactionDBMapper.Map<Transaction>(transactionRecord);
            }
        }

        public IList<Transaction> GetTransactions(string bankId, string accountId)
        {
            IList<Transaction> transactions;
            using (BankContext bankContext = new BankContext())
            {
                transactions = transactionDBMapper.Map<Transaction[]>(bankContext.Transaction.Where(t => t.BankId == bankId && t.AccountId == accountId).ToList());
            }
            if (transactions.Count == 0 || transactions == null)
            {
                throw new NoTransactionsException();
            }
            return transactions;
        }
    }
}

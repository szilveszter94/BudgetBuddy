namespace BudgetBuddy.Services.Repositories.Account;

using Model;
using Contracts.ModelRequest;
using Contracts.ModelRequest.UpdateModels;
using Data;
using Microsoft.EntityFrameworkCore;

public class AccountRepository : IAccountRepository
{
    private readonly BudgetBuddyContext _budgetBuddyContext;
    public AccountRepository(BudgetBuddyContext budgetBuddyContext)
    {
        _budgetBuddyContext = budgetBuddyContext;
    }

    public async Task<List<Account>> GetAll()
    {
        try
        {
            return await _budgetBuddyContext.Accounts.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Cannot get accounts.");
        }
    }
    
    public async Task<Account> GetById(int id)
    {
        try
        {
            var result = await _budgetBuddyContext.Accounts.FirstOrDefaultAsync(acc => acc.Id == id);
            if (result == null)
            {
                throw new KeyNotFoundException("Account not found.");
            }

            return result;
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("An unexpected error occured, cannot get account");
        }
    }

    public async Task<List<Account>> GetByUserId(string userId)
    {
        try
        {
            var results = await _budgetBuddyContext.Accounts.Where(acc => acc.UserId == userId).ToListAsync();
            return results;
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("An unexpected error occured, cannot get account");
        }
    }


    public async Task<Account> CreateAccount(AccountCreateRequest account)
    {
        try
        {
            var accountToAdd = new Account
            {
                Balance = account.Balance,
                Date = DateTime.Now,
                Name = account.Name,
                Type = account.Type,
                UserId = account.UserId
            };
            var newAccount = await _budgetBuddyContext.Accounts.AddAsync(accountToAdd);
            await _budgetBuddyContext.SaveChangesAsync();
            return newAccount.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Cannot create new account.");
        }
    }


    public async Task<Account> UpdateAccount(AccountUpdateRequest account)
    {
        try
        {
            var existingAccount = await _budgetBuddyContext.Accounts.FirstOrDefaultAsync(c => c.Id == account.Id);

            if (existingAccount == null)
            {
                throw new KeyNotFoundException("Failed to update. Account not found.");
            }
            
            _budgetBuddyContext.Entry(existingAccount).CurrentValues.SetValues(account);
            await _budgetBuddyContext.SaveChangesAsync();

            return existingAccount;
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("An unexpected error occured. Account not updated.");
        }
    }

    public async Task DeleteAccount(int id)
    {
        try
        {
            var accountToDelete = await _budgetBuddyContext.Accounts.FirstOrDefaultAsync(c => c.Id == id);

            if (accountToDelete == null)
            {
                throw new KeyNotFoundException("Failed to delete. Account not found.");
            }
            
            _budgetBuddyContext.Accounts.Remove(accountToDelete);
            await _budgetBuddyContext.SaveChangesAsync();
        }
        catch (KeyNotFoundException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception("Account not deleted. An unexpected error occured.");
        }
    }
}
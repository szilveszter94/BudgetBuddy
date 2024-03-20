using BudgetBuddy.Model;
using BudgetBuddy.Model.CreateModels;

namespace BudgetBuddy.Services.Repositories.Goal;

public interface IGoalRepository
{
    Task<GoalModel[]> GetAllGoalsByAccountId(int accountId, bool? completed = null);
    Task<GoalModel> CreateGoal(GoalInputModel goal);
    Task<GoalModel> UpdateGoal(GoalModel goal);
    Task DeleteGoal(int id);
}
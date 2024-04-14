using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programmin2_classroom.Shared.Models.present.toShow;
using TriangleDbRepository;

namespace Programmin2_classroom.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresentController : ControllerBase
    {
        private readonly DbRepository _db;

        public PresentController(DbRepository db)
        {
            _db = db;
        }

        [HttpGet("userToShow/{userID}")] // שליפה של התצוגה הראשונית בסביבה השניה לפני לחיצות על כפתורים
        public async Task<IActionResult> GetUser(int userID)
        {
            object param = new
            {
                ID = userID
            };

            string GetUserQuery = "SELECT id, firstName, profilePicOrIcon FROM users WHERE id = @ID";
            string GetCategoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE userID = @ID";
            string GetSpendingsQuery = "SELECT SUM(transValue) FROM transactions WHERE userID = @ID AND transType = 1";
            string GetIncomesQuery = "SELECT SUM(transValue) FROM transactions WHERE userID = @ID AND transType = 2";

            var recordUser = await _db.GetRecordsAsync<userToShow>(GetUserQuery, param);
            userToShow user = recordUser.FirstOrDefault();

            if (user != null)
            {
                var recordsCategrories = await _db.GetRecordsAsync<CategoryToShow>(GetCategoryQuery, param);
                user.categoriesFullList = recordsCategrories.ToList();

                double budgetSum = 0;

                if (user.categoriesFullList != null)
                {
                    
                    foreach (var category in user.categoriesFullList)
                    {
                        string GetBudgetSumQuery = "SELECT SUM(monthlyPlannedBudget) FROM subcategories WHERE categoryID = @categoryID";
                        object subCategoryParam = new
                        {
                            categoryID = category.id
                        };

                        var recordsBudget = await _db.GetRecordsAsync<double>(GetBudgetSumQuery, subCategoryParam);
                        double budgetValue = recordsBudget.FirstOrDefault();
                        user.budgetFullValue += budgetValue; // Add to the total sum
                        budgetSum = user.budgetFullValue;

                    }

                }    
                var recordsSpendings = await _db.GetRecordsAsync<double>(GetSpendingsQuery, param);
                user.spendingValueFullList = recordsSpendings.FirstOrDefault();

                user.budgetFullValue = Math.Round((budgetSum / user.spendingValueFullList) * 100); 

                var recordsIncomes = await _db.GetRecordsAsync<double>(GetIncomesQuery, param);
                user.incomeValueFullList = recordsIncomes.FirstOrDefault();

                return Ok(user);
            }
            return BadRequest("user not found");
        }


        [HttpGet("subCategoryToShow/{categoryID}")]
        public async Task<IActionResult> subCategoryToShow(int categoryID)
        {
            object param = new
            {
                ID = categoryID
            };

            string GetSubCategoriesQuery = "SELECT id, subCategoryTitle, monthlyPlannedBudget FROM subcategories WHERE categoryID = @ID";
            var recordsubCategories = await _db.GetRecordsAsync<SubCategoryToShow>(GetSubCategoriesQuery, param);
            List<SubCategoryToShow> subCategories = recordsubCategories.ToList();

            if (subCategories.Count > 0)
            {
                foreach (var subCategory in subCategories)
                {
                    if (subCategory.id != null)
                    {
                        object subParam = new
                        {
                            ID = subCategory.id
                        };

                        string GetTransactionValueQuery = "SELECT COALESCE(SUM(transValue), 0) FROM transactions WHERE subCategoryID = @ID AND transType = 1";

                        var recordsTransValue = await _db.GetRecordsAsync<double>(GetTransactionValueQuery, subParam);
                        subCategory.transactionsValue = recordsTransValue.FirstOrDefault();
                    }
                    else
                    {
                        return BadRequest("no transaction in sub category");
                    }
                   
                }

                return Ok(subCategories);
            }

            return BadRequest("user not found");
        }
    }

}  




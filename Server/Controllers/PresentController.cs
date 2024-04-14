using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Programmin2_classroom.Shared.Models.present.toAdd;
using Programmin2_classroom.Shared.Models.present.toEdit;
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


        [HttpGet("GetCategoryToEdit/{CategoryId}")] // שליפת קטגוריה לעריכה
        public async Task<IActionResult> GetFullGame(int CategoryId)
        {

            object param = new
            {
                ID = CategoryId
            };

            string GetCategoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE id = @ID";

            var recordCategory = await _db.GetRecordsAsync<CategoryToShow>(GetCategoryQuery, param);
            CategoryToShow category = recordCategory.FirstOrDefault();

            if (category != null)
            {
                return Ok(category);
            }
            return BadRequest("category not found");
        }



        [HttpPost("EditCategory")]  // עריכת קטגוריה

        public async Task<IActionResult> editCategory(CategoryToEdit categoryToUpdate)
        {

            object updateParam = new
            {
                ID = categoryToUpdate.id,
                categroyTitle = categoryToUpdate.categroyTitle,
                icon = categoryToUpdate.icon,
                color = categoryToUpdate.color
            };

            string UpdateCategoryQuery = "UPDATE categories set categroyTitle = @categroyTitle, icon = @icon, color = @color where id =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateCategoryQuery, updateParam);

            if (isUpdate)
            {
                return Ok(categoryToUpdate);
            }
            return BadRequest("update category faild");
        }



        [HttpPost("AddCategory/{userID}")] // יצירת קטגוריה חדשה
        public async Task<IActionResult> AddCategory(int userID, CategoryToAdd categoryToAdd) // לראות איך היוזר אידי מגיע מהפרונט אנד 
        {
            object categoryToAddParam = new
            {
                userID = userID,
                categroyTitle = categoryToAdd.categroyTitle,
                icon = categoryToAdd.icon,
                color = categoryToAdd.color
            };

            string insertCategoryQuery = "INSERT INTO categories (categroyTitle,userID, icon, color) values (@categroyTitle ,@userID ,@icon ,@color)";

            int newCategoryId = await _db.InsertReturnId(insertCategoryQuery, categoryToAddParam);

            if (newCategoryId != 0)
            {
                object param = new
                {
                    id = newCategoryId,
                    userID = categoryToAdd.userID
                };

                string GetCategoryQuery = "SELECT id, categroyTitle, icon, color FROM categories WHERE id = @id AND userID = @userID";
                var recordsCategory = await _db.GetRecordsAsync<CategoryToShow>(GetCategoryQuery, param);
                CategoryToShow category = recordsCategory.FirstOrDefault();

                if (category != null)
                {
                    return Ok(category);
                }
                return BadRequest("category not found");

            }

            return BadRequest("category not created");
        }

    }
}





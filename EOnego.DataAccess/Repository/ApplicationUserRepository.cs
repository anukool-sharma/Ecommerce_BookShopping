using EOnego.DataAccess.Data;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOnego.DataAccess.Repository
{
    public class ApplicationUserRepository:Repository<ApplicationUser>,IApplicationUserRepository
    {
        private readonly ApplicationDbContext _context;
        public ApplicationUserRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
    }
}

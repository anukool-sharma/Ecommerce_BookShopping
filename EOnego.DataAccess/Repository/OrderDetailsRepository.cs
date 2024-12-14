using EOnego.DataAccess.Data;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOnego.DataAccess.Repository
{
    public class OrderDetailsRepository:Repository<OrderDetails>,IOrderDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailsRepository(ApplicationDbContext context):base(context)
        {
            _context = context;
        }
    }
}

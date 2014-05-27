using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace Tarea.Datos
{
    public interface IUnitOfWork : IDisposable {
        void Commit();
        IDatabase bD { get; }
    }

    public class Uow : IUnitOfWork, IDisposable
    {
        private readonly ITransaction _nTransaction;
        public readonly IDatabase _bd;
        private bool disposed = false;

        public Uow(IDatabase db)
        {
            _bd = db;
            _nTransaction = _bd.GetTransaction();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (!disposing)
                {
                    _nTransaction.Dispose();
                }
                 
                
            }
            disposed = true;
            
        }

       public void Dispose()
       {
          
           Dispose(true);
           GC.SuppressFinalize(this);
       }

        public IDatabase bD
        {
            get { return _bd; }
        }

        public void Commit()
        {
            _nTransaction.Complete();
        }
    }
}

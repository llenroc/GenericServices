﻿using System;
using System.Data.Entity;

namespace GenericServices.Concrete
{
    public class UpdateService<TData> : IUpdateService<TData> where TData : class
    {
        private readonly IDbContextWithValidation _db;

        public UpdateService(IDbContextWithValidation db)
        {
            _db = db;
        }

        public ISuccessOrErrors Update(TData itemToUpdate)
        {
            if (itemToUpdate == null)
                throw new ArgumentNullException("itemToUpdate", "The item provided was null.");

            //Set the entry as modified
            _db.Entry(itemToUpdate).State = EntityState.Modified;

            var result = _db.SaveChangesWithValidation();
            if (result.IsValid)
                result.SetSuccessMessage("Successfully updated {0}.", typeof(TData).Name);

            return result;
        }
    }

    //------------------------------------------------------------------------

    public class UpdateService<TData, TDto> : IUpdateService<TData, TDto>
        where TData : class
        where TDto : EfGenericDto<TData, TDto>
    {
        private readonly IDbContextWithValidation _db;

        public UpdateService(IDbContextWithValidation db)
        {
            _db = db;
        }

        public ISuccessOrErrors Update(TDto dto)
        {
            ISuccessOrErrors result = new SuccessOrErrors();
            if (!dto.SupportedFunctions.HasFlag(CrudFunctions.Update))
                return result.AddSingleError("Delete of a {0} is not supported in this mode.", dto.DataItemName);

            var itemToUpdate = dto.FindItemTracked(_db);
            if (itemToUpdate == null)
                return result.AddSingleError("Could not find the {0} you requested.", dto.DataItemName);

            result = dto.CopyDtoToData(_db, dto, itemToUpdate); //update those properties we want to change
            if (!result.IsValid)
                return result;

            result = _db.SaveChangesWithValidation();
            if (result.IsValid)
                result.SetSuccessMessage("Successfully updated {0}.", dto.DataItemName);

            return result;

        }

    }

}
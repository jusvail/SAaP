﻿using SAaP.Core.Models.DB;

namespace SAaP.Contracts.Services
{
    public interface IDbTransferService
    {
        Task TransferCsvDataToDb(IEnumerable<string> codeNames);

        Task StoreActivityDataToDb(ActivityData activity);

        Task StoreActivityDataToDb(DateTime now, string queryString, string data);
    }
}

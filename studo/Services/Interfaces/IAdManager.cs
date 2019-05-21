﻿using Microsoft.AspNetCore.Mvc;
using studo.Models;
using studo.Models.Requests.Ads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Interfaces
{
    public interface IAdManager
    {
        IQueryable<Ad> Ads { get; }
        Task<IQueryable<Ad>> AddAsync(AdCreateRequest adCreateRequest);
        Task<IQueryable<Ad>> EditAsync(AdEditRequest adEditRequest);
        Task DeleteAsync(Guid adId);
    }
}

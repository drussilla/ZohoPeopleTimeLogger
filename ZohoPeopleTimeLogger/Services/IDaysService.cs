﻿using System;
using System.Collections.Generic;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDaysService
    {
        List<DayViewModel> GetDays(DateTime month);
    }
}
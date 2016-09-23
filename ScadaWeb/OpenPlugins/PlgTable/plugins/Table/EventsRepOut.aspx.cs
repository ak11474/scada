﻿/*
 * Copyright 2016 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : PlgTable
 * Summary  : Events report output web form
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2016
 * Modified : 2016
 */

using Scada.Client;
using System;
using Utils.Report;

namespace Scada.Web.Plugins.Table
{
    /// <summary>
    /// Events report output web form
    /// <para>Выходная веб-форма отчёта по событиям</para>
    /// </summary>
    /// <remarks>
    /// URL example: 
    /// http://webserver/scada/plugins/Table/EventsRepOut.aspx?viewID=1&year=2016&month=1&day=1
    /// </remarks>
    public partial class WFrmEventsRepOut : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AppData appData = AppData.GetAppData();
            UserData userData = UserData.GetUserData();

            // получение ид. представления из параметров запроса
            int viewID;
            int.TryParse(Request.QueryString["viewID"], out viewID);
            bool eventsByView = viewID > 0;

            // проверка прав
            if (!(userData.LoggedOn && (userData.UserRights.ViewAllRight ||
                eventsByView && userData.UserRights.GetUiObjRights(viewID).ViewRight)))
                throw new ScadaException(CommonPhrases.NoRights);

            // загрузка представления
            BaseView view;
            if (eventsByView)
            {
                Type viewType = userData.UserViews.GetViewType(viewID);
                view = appData.ViewCache.GetView(viewType, viewID, true);
            }
            else
            {
                view = null;
            }

            // получение даты запрашиваемых событий
            DateTime reqDate = WebUtils.GetDateFromQueryString(Request);

            // генерация отчёта
            RepBuilder repBuilder = new EventsRepBuilder(appData.DataAccess);
            RepUtils.WriteGenerationAction(appData.Log, repBuilder, userData);
            RepUtils.GenerateReport(
                repBuilder, 
                new object[] { view, reqDate }, 
                Server.MapPath("~/plugins/Table/templates/"), 
                RepUtils.BuildFileName("Events", "xml"), 
                Response);
        }
    }
}
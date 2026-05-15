using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;
using Newtonsoft.Json;
using static CitizenFX.Core.Native.API;
using hbAdminLiteClient.data;

namespace hbAdminLiteClient
{
    public partial class AdminLiteClient : BaseScript
    {
        private async Task RestorePedAfterModelChangeAsync(uint modelHash)
        {
            var ped = Game.PlayerPed;
            if (ped == null || !ped.Exists())
            {
                return;
            }

            SetPedDefaultComponentVariation(ped.Handle);
            ClearAllPedProps(ped.Handle);
            ClearPedDecorations(ped.Handle);
            ClearPedFacialDecorations(ped.Handle);

            if (modelHash == (uint)GetHashKey("mp_f_freemode_01") || modelHash == (uint)GetHashKey("mp_m_freemode_01"))
            {
                SetPedHeadBlendData(ped.Handle, 0, 0, 0, 0, 0, 0, 0.5f, 0.5f, 0f, false);
                while (!HasPedHeadBlendFinished(ped.Handle))
                {
                    await Delay(0);
                }
            }

            RestoreLocalPlayerPedVisibility();
        }

    }
}

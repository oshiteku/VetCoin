using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VetCoin.Data;
using VetCoin.Services;

namespace VetCoin.Pages
{
    public class MemberModel : PageModel
    {
        public MemberModel(CoreService coreService)
        {
            CoreService = coreService;
        }

        public CoreService CoreService { get; }

        public long Amount { get; set; }

        public UserContext UserContext { get; set; }

        public VetMember VetMember { get; set; }

        public IEnumerable<Trade> Trades { get; set; }

        public IEnumerable<Contract> Contracts { get; set; }

        public IEnumerable<CoinTransaction> Transactions { get; set; }

        public void OnGet(ulong memberId)
        {
            UserContext = CoreService.GetUserContext();

            VetMember = CoreService.DbContext.VetMembers.FirstOrDefault(c => c.DiscordId == memberId);
            Amount = CoreService.CalcAmount(VetMember);

            Transactions = CoreService
                                .GetCoinTransactionQuery(VetMember)
                                .Include(c => c.RecivedVetMember)
                                .Include(c => c.SendVetMember)
                                .OrderByDescending(c => c.CreateDate);

            Trades = CoreService.DbContext
                            .Trades
                            .Include(c => c.VetMember)
                            .Include(c => c.TradeMessages)
                                .ThenInclude(c => c.VetMember)
                            .OrderByDescending(c => c.TradeMessages.Max(d => d.CreateDate))
                            .ThenByDescending(c => c.CreateDate)
                            .Where(c => c.VetMemberId == VetMember.Id)
                            .Take(5)
                            .ToArray();

            Contracts = CoreService.DbContext
                                .Contracts
                                .Include(c => c.VetMember)
                                .Include(c => c.Trade.VetMember)
                                .Where(c => c.VetMemberId == VetMember.Id)
                                .OrderByDescending(c => c.ContractMessages.Max(d => d.CreateDate))
                                .Take(5)
                                .ToArray();
        }
    }
}

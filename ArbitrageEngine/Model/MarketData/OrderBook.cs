using System.Linq;
using System.Collections.Generic;

namespace ArbitrageEngine.Model
{
	public class OrderBook
	{
		public List<OrderDetail> AskDetails { get; private set; }
		public List<OrderDetail> BidDetails { get; private set; }

		public void Clear()
		{
			AskDetails.Clear();
			BidDetails.Clear();
		}

		public void SetBids(IEnumerable<OrderDetail> bids)
		{
			BidDetails = bids.OrderByDescending(s => s.Price).ToList();
		}

		public void SetAsks(IEnumerable<OrderDetail> asks)
		{
			AskDetails = asks.OrderBy(s => s.Price).ToList();
		}

		public OrderDetail GetBid(int depth)
		{
			return BidDetails[depth];
		}

		public OrderDetail GetAsk(int depth)
		{
			return AskDetails[depth];
		}

		public int AskDepth 
        {
			get { return AskDetails == null ? 0 : AskDetails.Count; }
		}

		public int BidDepth 
        {
			get { return BidDetails == null ? 0 : BidDetails.Count; }
		}

        public bool HasAsks
        {
            get { return AskDepth > 0; }
        }

        public bool HasBids
        {
            get { return BidDepth > 0; }
        }

        internal decimal GetBidAmount(int depth)
        {
			return BidDetails[depth].Quantity;
        }

        internal decimal GetAskAmount(int depth)
        {
			return AskDetails[depth].Quantity;
		}
	}       
}
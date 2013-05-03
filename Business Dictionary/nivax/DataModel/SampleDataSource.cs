using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace nivax.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : nivax.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}",
                        "nivax");

            var group1 = new SampleDataGroup("Group-1",
                    "Introduction to Business",
                    "",
                    "Assets/10.png",
                    "");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Business",
                    "",
                    "Assets/11.png",
                    "A business (also known as enterprise or firm) is an organization involved in the trade of goods, services, or both to consumers.[1] Business plan and Business model determine the outcome of an active business operation.",
                    "A business (also known as enterprise or firm) is an organization involved in the trade of goods, services, or both to consumers.[1] Business plan and Business model determine the outcome of an active business operation. Businesses are predominant in capitalist economies, where most of them are privately owned and administered to earn profit to increase the wealth of their owners. Businesses may also be not-for-profit or state-owned. A business owned by multiple individuals may be referred to as a company, although that term also has a more precise meaning.\n\nThe etymology of business relates to the state of being busy either as an individual or society, as a whole, doing commercially viable and profitable work. The term business has at least three usages, depending on the scope — the singular usage to mean a particular organization; the generalized usage to refer to a particular market sector, the music business and compound forms such as agribusiness; and the broadest meaning, which encompasses all activity by the community of suppliers of goods and services. However, the exact definition of business, like much else in the philosophy of business, is a matter of debate and complexity of meanings.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "Basic forms of ownership",
                    "",
                    "Assets/12.png",
                    "Although forms of business ownership vary by jurisdiction, several common forms exist:",
                    "Sole proprietorship: A sole proprietorship is a business owned by one person for-profit. The owner may operate the business alone or may employ others. The owner of the business has unlimited liability for the debts incurred by the business.\n\nPartnership: A partnership is a business owned by two or more people. In most forms of partnerships, each partner has unlimited liability for the debts incurred by the business. The three typical classifications of for-profit partnerships are general partnerships, limited partnerships, and limited liability partnerships.\n\nCorporation: A corporation is a limited liability business that has a separate legal personality from its members. Corporations can be either government-owned or privately owned, and corporations can organize either for-profit or not-for-profit. A privately owned, for-profit corporation is owned by shareholders who elect a board of directors to direct the corporation and hire its managerial staff. A privately owned, for-profit corporation can be either privately held or publicly held.\n\nCooperative: Often referred to as a co-op, a cooperative is a limited liability business that can organize for-profit or not-for-profit. A cooperative differs from a for-profit corporation in that it has members, as opposed to shareholders, who share decision-making authority. Cooperatives are typically classified as either consumer cooperatives or worker cooperatives. Cooperatives are fundamental to the ideology of economic democracy.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Classifications",
                    "",
                    "Assets/13.png",
                    "Agriculture and mining businesses are concerned with the production of raw material, such as plants or minerals.",
                    "Agriculture and mining businesses are concerned with the production of raw material, such as plants or minerals.\nFinancial businesses include banks and other companies that generate profit through investment and management of capital.\nInformation businesses generate profits primarily from the resale of intellectual property and include movie studios, publishers and packaged software companies.\nManufacturers produce products, from raw materials or component parts, which they then sell at a profit. Companies that make physical goods, such as cars or pipes, are considered manufacturers.\nReal estate businesses generate profit from the selling, renting, and development of properties comprising land, residential homes, and other kinds of buildings.\nRetailers and distributors act as middle-men in getting goods produced by manufacturers to the intended consumer, generating a profit as a result of providing sales or distribution services. Most consumer-oriented stores and catalog companies are distributors or retailers.\nService businesses offer intangible goods or services and typically generate a profit by charging for labor or other services provided to government, other businesses, or consumers. Organizations ranging from house decorators to consulting firms, restaurants, and even entertainers are types of service businesses.\nTransportation businesses deliver goods and individuals from location to location, generating a profit on the transportation costs.\nUtilities produce public services such as electricity or sewage treatment, usually under a government charter.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Management",
                    "",
                    "Assets/14.png",
                    "The efficient and effective operation of a business, and study of this subject, is called management. The major branches of management are financial management, marketing management, human resource management, strategic management, production management, operations management, service management and information technology management.",
                    "The efficient and effective operation of a business, and study of this subject, is called management. The major branches of management are financial management, marketing management, human resource management, strategic management, production management, operations management, service management and information technology management.\n\nOwners engage in business administration either directly or indirectly through the employment of managers. Owner managers, or hired managers administer to three component resources that constitute the business' value or worth: financial resources, capital or tangible resources, and human resources. These resources are administered to in at least five functional areas: legal contracting, manufacturing or service production, marketing, accounting, financing, and human resourcing.",
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Reforming state enterprises",
                    "",
                    "Assets/15.png",
                    "In recent decades, assets and enterprises that were run by various states have been modeled after business enterprises. In 2003, the People's Republic of China reformed 80% of its state-owned enterprises and modeled them on a company-type management system.",
                    "In recent decades, assets and enterprises that were run by various states have been modeled after business enterprises. In 2003, the People's Republic of China reformed 80% of its state-owned enterprises and modeled them on a company-type management system. Many state institutions and enterprises in China and Russia have been transformed into joint-stock companies, with part of their shares being listed on public stock markets.\n\nBusiness process management (BPM) is a holistic management approach[1] focused on aligning all aspects of an organization with the wants and needs of clients. It promotes business effectiveness and efficiency while striving for innovation, flexibility, and integration with technology. BPM attempts to improve processes continuously. It can therefore be described as a process optimization process It is argued that BPM enables organizations to be more efficient, more effective and more capable of change than a functionally focused, traditional hierarchical management approach.",
                    group1));
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "Impact",
                    "",
                    "Assets/20.png",
                    "");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Theory of the firm",
                    "",
                    "Assets/21.png",
                    "The theory of the firm consists of a number of economic theories that describe, explain, and predict the nature of the firm, company, or corporation, including its existence, behavior, structure, and relationship to the market.",
                    "Firms exist as an alternative system to the market-price mechanism when it is more efficient to produce in a non-market environment. For example, in a labor market, it might be very difficult or costly for firms or organizations to engage in production when they have to hire and fire their workers depending on demand/supply conditions. It might also be costly for employees to shift companies every day looking for better alternatives. Similarly, it may be costly for companies to find new suppliers daily. Thus, firms engage in a long-term contract with their employees or a long-term contract with suppliers to minimize the cost or maximize the value of property rights.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "Commercial law",
                    "",
                    "Assets/22.png",
                    "Commercial law, also known as business law, is the body of law that applies to the rights, relations, and conduct of persons and businesses engaged in commerce, merchandising, trade, and sales.",
                    "Commercial law, also known as business law, is the body of law that applies to the rights, relations, and conduct of persons and businesses engaged in commerce, merchandising, trade, and sales. It is often considered to be a branch of civil law and deals with issues of both private law and public law.\n\nCommercial law includes within its compass such titles as principal and agent; carriage by land and sea; merchant shipping; guarantee; marine, fire, life, and accident insurance; bills of exchange and partnership. It can also be understood to regulate corporate contracts, hiring practices, and the manufacture and sales of consumer goods. Many countries have adopted civil codes that contain comprehensive statements of their commercial law.\n\nIn the United States, commercial law is the province of both the United States Congress, under its power to regulate interstate commerce, and the states, under their police power. Efforts have been made to create a unified body of commercial law in the United States; the most successful of these attempts has resulted in the general adoption of the Uniform Commercial Code, which has been adopted in all 50 states (with some modification by state legislatures), the District of Columbia, and the U.S. territories.\n\nVarious regulatory schemes control how commerce is conducted, particularly vis-a-vis employees and customers. Privacy laws, safety laws (e.g., the Occupational Safety and Health Act in the United States), and food and drug laws are some examples.",
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Stock market",
                    "",
                    "Assets/23.png",
                    "A stock market or equity market is a public entity (a loose network of economic transactions, not a physical facility or discrete entity) for the trading of company stock (shares) and derivatives at an agreed price; these are securities listed on a stock exchange as well as those only traded privately.",
                    "A stock market or equity market is a public entity (a loose network of economic transactions, not a physical facility or discrete entity) for the trading of company stock (shares) and derivatives at an agreed price; these are securities listed on a stock exchange as well as those only traded privately.\n\nThe size of the world stock market was estimated at about $36.6 trillion at the beginning of October 2008. The total world derivatives market has been estimated at about $791 trillion face or nominal value, 11 times the size of the entire world economy. The value of the derivatives market, because it is stated in terms of notional values, cannot be directly compared to a stock or a fixed income security, which traditionally refers to an actual value. Moreover, the vast majority of derivatives 'cancel' each other out (i.e., a derivative 'bet' on an event occurring is offset by a comparable derivative 'bet' on the event not occurring). Many such relatively illiquid securities are valued as marked to model, rather than an actual market price.",
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "Directions",
                    "",
                    "Assets/30.png",
                    "");
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "Stock exchange",
                    "",
                    "Assets/31.png",
                    "A stock exchange is a form of exchange which provides services for stock brokers and traders to trade stocks, bonds, and other securities. Stock exchanges also provide facilities for issue and redemption of securities and other financial instruments, and capital events including the payment of income and dividends.",
                    "A stock exchange is a form of exchange which provides services for stock brokers and traders to trade stocks, bonds, and other securities. Stock exchanges also provide facilities for issue and redemption of securities and other financial instruments, and capital events including the payment of income and dividends. Securities traded on a stock exchange include shares issued by companies, unit trusts, derivatives, pooled investment products and bonds.\n\nTo be able to trade a security on a certain stock exchange, it must be listed there. Usually, there is a central location at least for record keeping, but trade is increasingly less linked to such a physical place, as modern markets are electronic networks, which gives them advantages of increased speed and reduced cost of transactions. Trade on an exchange is by members only.\n\nThe initial offering of stocks and bonds to investors is by definition done in the primary market and subsequent trading is done in the secondary market. A stock exchange is often the most important component of a stock market. Supply and demand in stock markets are driven by various factors that, as in all free markets, affect the price of stocks (see stock valuation).",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "Corporate governance",
                    "",
                    "Assets/32.png",
                    "By having a wide and varied scope of owners, companies generally tend to improve management standards and efficiency to satisfy the demands of these shareholders, and the more stringent rules for public corporations imposed by public stock exchanges and the government.",
                    "By having a wide and varied scope of owners, companies generally tend to improve management standards and efficiency to satisfy the demands of these shareholders, and the more stringent rules for public corporations imposed by public stock exchanges and the government. Consequently, it is alleged that public companies (companies that are owned by shareholders who are members of the general public and trade shares on public exchanges) tend to have better management records than privately held companies (those companies where shares are not publicly traded, often owned by the company founders and/or their families and heirs, or otherwise by a small group of investors).",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "Capital intensity",
                    "",
                    "Assets/33.png",
                    "Capital intensity is the term for the amount of fixed or real capital present in relation to other factors of production, especially labor. At the level of either a production process or the aggregate economy, it may be estimated by the capital/labor ratio, such as from the points along a capital/labor isoquant.",
                    "Calculations made by Solow claimed that economic growth was mainly driven by technological progress (productivity growth) rather than inputs of capital and labor. However recent economic research has invalidated that theory, since Solow did not properly consider changes in both investment and labor inputs.\n\nDale Jorgenson, of Harvard University, President of the American Economic Association in 2000, concludes that: ‘Griliches and I showed that changes in the quality of capital and labor inputs and the quality of investment goods explained most of the Solow residual. We estimated that capital and labor inputs accounted for 85 percent of growth during the period 1945–1965, while only 15 percent could be attributed to productivity growth… This has precipitated the sudden obsolescence of earlier productivity research employing the conventions of Kuznets and Solow. \n\nJohn Ross has analysed the long term correlation between the level of investment in the economy, rising from 5-7% of GDP at the time of the Industrial Revolution in England, to 25% of GDP in the post-war German economic miracle, to over 35% of GDP in the world’s most rapidly growing contemporary economies of India and China.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-4",
                    "Startup company",
                    "",
                    "Assets/34.png",
                    "A startup company or startup is a company, a partnership or temporary organization designed to search for a repeatable and scalable business model. These companies, generally newly created, are in a phase of development and research for markets.",
                    "A startup company or startup is a company, a partnership or temporary organization designed to search for a repeatable and scalable business model. These companies, generally newly created, are in a phase of development and research for markets. The term became popular internationally during the dot-com bubble when a great number of dot-com companies were founded.\n\nLately, the term startup has been associated mostly with technological ventures designed for high-growth. Paul Graham, founder of one of the top startup accelerators in the world, defines a startup as: A startup is a company designed to grow fast. Being newly founded does not in itself make a company a startup. Nor is it necessary for a startup to work on technology, or take venture funding, or have some sort of exit The only essential thing is growth. Everything else we associate with startups follows from growth.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-5",
                    "Capital accumulation",
                    "",
                    "Assets/35.png",
                    "The accumulation of capital is the gathering or amassing of objects of value; the increase in wealth through concentration; or the creation of wealth.",
                    "The accumulation of capital is the gathering or amassing of objects of value; the increase in wealth through concentration; or the creation of wealth. Capital is money or a financial asset invested for the purpose of making more money (whether in the form of profit, rent, interest, royalties, capital gain or some other kind of return). This activity forms the basis of the economic system of capitalism, where economic activity is structured around the accumulation of capital (investment in order to realize a financial profit).Human capital may also be seen as a form of capital: investment in one's personal abilities, such as through education, to improve their function and therefore increase their income potential in a market economy.",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-6",
                    "Entrepreneur",
                    "",
                    "Assets/36.png",
                    "The term entrepreneur is a loanword from French, and is commonly used to describe an individual who organizes and operates a business or businesses, taking on financial risk to do so.",
                    "The term entrepreneur is a loanword from French, and is commonly used to describe an individual who organizes and operates a business or businesses, taking on financial risk to do so. The term was first defined by the Irish-French economist Richard Cantillon as the person who pays a certain price for a product to resell it at an uncertain price, thereby making decisions about obtaining and using the resources while consequently admitting the risk of enterprise. The term first appeared in the French Dictionary, Dictionnaire Universel de Commerce of Jacques des Bruslons published",
                    group3));
            group3.Items.Add(new SampleDataItem("Group-3-Item-7",
                    "Corporation",
                    "",
                    "Assets/37.png",
                    "A corporation is a separate legal entity that has been incorporated through a legislative or registration process established through legislation.",
                    "A corporation is a separate legal entity that has been incorporated through a legislative or registration process established through legislation. Incorporated entities have legal rights and liabilities that are distinct from their employees and shareholders, and may conduct business as either a profit-seeking business or not for profit business. Early incorporated entities were established by charter (i.e. by an ad hoc act granted by a monarch or passed by a parliament or legislature). Most jurisdictions now allow the creation of new corporations through registration. In addition to legal personality, registered companies tend to have limited liability, be owned by shareholders who can transfer their shares to others, and controlled by a board of directors whom the shareholders appoint.",
                    group3));
            this.AllGroups.Add(group3);
        }
    }
}

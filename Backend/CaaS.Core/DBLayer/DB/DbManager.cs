using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.DB
{
    public class DbManager
    {
        private readonly IConnectionFactory connectionFactory;
        public DbManager(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        /// <summary>
        /// creates all tables. also drops them first if exist.
        /// </summary>
        /// <returns></returns>
        public async Task CreateAsync()
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "\r\n\r\n-- Drop all tables\r\nDROP TABLE IF EXISTS Customer;\r\nDROP TABLE IF EXISTS OrderProduct;\r\nDROP TABLE IF EXISTS OrderEntry;\r\nDROP TABLE IF EXISTS [Order];\r\nDROP TABLE IF EXISTS ProductDiscountProduct;\r\nDROP TABLE IF EXISTS DiscountFreeProduct;\r\nDROP TABLE IF EXISTS Discount;\r\nDROP TABLE IF EXISTS CartEntry;\r\nDROP TABLE IF EXISTS Cart;\r\nDROP TABLE IF EXISTS Product;\r\nDROP TABLE IF EXISTS Shop;\r\n\r\n\r\n-- Create all tables\r\nCREATE TABLE [dbo].[Shop] (\r\n    [Id]     UNIQUEIDENTIFIER   DEFAULT newid(),\r\n    [Name]   VARCHAR (50)       NOT NULL,\r\n    [AppKey] VARCHAR (50)       NULL,\r\n    PRIMARY KEY CLUSTERED ([Id] ASC)\r\n);\r\n\r\nCREATE TABLE [dbo].[Cart] (\r\n    [Id]        UNIQUEIDENTIFIER    DEFAULT newid(),\r\n    [ShopId]    UNIQUEIDENTIFIER    NOT NULL,\r\n    PRIMARY KEY CLUSTERED ([Id] ASC),\r\n    CONSTRAINT [fk_shop_cart] FOREIGN KEY ([ShopId])\r\n        REFERENCES [dbo].[Shop] (Id) ON DELETE CASCADE\r\n);\r\n\r\nCREATE TABLE [dbo].[Product] (\r\n    [Id]           UNIQUEIDENTIFIER DEFAULT newid(),\r\n    [ShopId]       UNIQUEIDENTIFIER NOT NULL,\r\n    [Price]        DECIMAL(18,2)    NOT NULL,\r\n    [Name]         VARCHAR (50)     NOT NULL,\r\n    [Description]  VARCHAR (MAX)    NULL,\r\n    [DownloadLink] VARCHAR (MAX)    NOT NULL,\r\n    [ImageNr]      INT              DEFAULT 0,            \r\n    PRIMARY KEY CLUSTERED ([Id] ASC),\r\n    CONSTRAINT [fk_shop_product] FOREIGN KEY ([ShopId]) \r\n        REFERENCES [dbo].[Shop] ([Id]) ON DELETE CASCADE\r\n);\r\n\r\n\r\nCREATE TABLE [dbo].[CartEntry] (\r\n    [ProductId]  UNIQUEIDENTIFIER NOT NULL,\r\n    [CartId]     UNIQUEIDENTIFIER NOT NULL,\r\n    [Count]      INT              NOT NULL,\r\n    CONSTRAINT [fk_product_cartentry] FOREIGN KEY ([ProductId]) \r\n    REFERENCES [dbo].[Product] ([Id]) ON DELETE NO ACTION,\r\n    CONSTRAINT [fk_cart_cartentry] FOREIGN KEY ([CartId]) \r\n    REFERENCES [dbo].[Cart] ([Id]) ON DELETE CASCADE,\r\n    PRIMARY KEY CLUSTERED  ([CartId] ASC, [ProductId] ASC)\r\n);\r\n\r\n-- discount tables\r\nCREATE TABLE [dbo].[Discount] (\r\n    [Id]                    UNIQUEIDENTIFIER    DEFAULT newid(),\r\n    [ShopId]                UNIQUEIDENTIFIER    NOT NULL,\r\n    [Tag]                   VARCHAR (50)        NOT NULL,\r\n    [Description]           VARCHAR (MAX)       NOT NULL,\r\n    [OffType]               INT                 NOT NULL,\r\n    [OffValue]              DECIMAL (18, 10)    NOT NULL,\r\n    [MinType]               INT                 NOT NULL,\r\n    [MinValue]              DECIMAL (10, 2)     NOT NULL,\r\n    [Is4AllProducts]        BIT                 NOT NULL,\r\n    [ValidFrom]             DATETIME            NOT NULL,\r\n    [ValidTo]               DATETIME            NOT NULL,\r\n\r\n    PRIMARY KEY CLUSTERED ([Id] ASC),\r\n    CONSTRAINT [fk_shop_discount] FOREIGN KEY ([ShopId]) \r\n    REFERENCES [dbo].[Shop] ([Id]) ON DELETE CASCADE\r\n);\r\n\r\nCREATE TABLE [dbo].[ProductDiscountProduct] (\r\n    [ProductId]  UNIQUEIDENTIFIER NOT NULL,\r\n    [DiscountId] UNIQUEIDENTIFIER NOT NULL,\r\n    CONSTRAINT [fk_product_productdiscountproduct] FOREIGN KEY ([ProductId]) \r\n    REFERENCES [dbo].[Product] ([Id])  ON DELETE CASCADE,\r\n    CONSTRAINT [fk_discount_productdiscountproduct] FOREIGN KEY ([DiscountId]) \r\n    REFERENCES [dbo].[Discount] ([Id]) ON DELETE NO ACTION,\r\n    PRIMARY KEY CLUSTERED ([ProductId] ASC, [DiscountId] ASC)\r\n);\r\n\r\nCREATE TABLE [dbo].[DiscountFreeProduct] (\r\n    [ProductId]  UNIQUEIDENTIFIER NOT NULL,\r\n    [DiscountId] UNIQUEIDENTIFIER NOT NULL,\r\n    [Count]      INT              NOT NULL,\r\n    CONSTRAINT [fk_product_discountfreeproduc] FOREIGN KEY ([ProductId]) \r\n    REFERENCES [dbo].[Product] ([Id]) ON DELETE CASCADE,\r\n    CONSTRAINT [fk_discount_discountfreeproduct] FOREIGN KEY ([DiscountId]) \r\n    REFERENCES [dbo].[Discount] ([Id]) ON DELETE NO ACTION,\r\n    PRIMARY KEY CLUSTERED ([ProductId] ASC, [DiscountId] ASC)\r\n);\r\n\r\n-- Order tables\r\nCREATE TABLE [dbo].[Order] (\r\n    [Id]            UNIQUEIDENTIFIER DEFAULT newid(),\r\n    [ShopId]        UNIQUEIDENTIFIER NOT NULL,\r\n\t[DateTime]\t    DATETIME\t\t NOT NULL,\r\n\t[OffSum]\t\tDECIMAL(18,2)\t NOT NULL,\r\n    [Total]         DECIMAL(18, 2)   NOT NULL,\r\n    [DownloadLink]  VARCHAR(MAX)     NOT NULL,\r\n    PRIMARY KEY CLUSTERED  ([Id] ASC),\r\n\tCONSTRAINT [fk_shop_order] FOREIGN KEY ([ShopId]) \r\n    REFERENCES [dbo].[Shop] ([Id]) ON DELETE NO ACTION\r\n);\r\n\r\nCREATE TABLE [dbo].[OrderEntry] (\r\n    [RowNr]      INT              NOT NULL,\r\n    [OrderId]    UNIQUEIDENTIFIER NOT NULL,\r\n    [Count]      INT              NOT NULL,\r\n    PRIMARY KEY CLUSTERED  ([RowNr] ASC, [OrderId] ASC),\r\n    CONSTRAINT [fk_order_orderentry] FOREIGN KEY ([OrderId])\r\n    REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE,\r\n);\r\n\r\nCREATE TABLE [dbo].[OrderProduct] (\r\n\t[RowNr]\t\t    INT\t\t\t\t    NOT NULL,\r\n\t[OrderId]\t    UNIQUEIDENTIFIER\tNOT NULL,\r\n\t[OriginalId]\tUNIQUEIDENTIFIER\tNOT NULL,\r\n\t[Price]\t\t    DECIMAL(18,2)\t    NOT NULL,\r\n\t[Name]\t\t    VARCHAR(50)\t\t    NOT NULL,\r\n\tPRIMARY KEY CLUSTERED ([RowNr] ASC, [OrderId] ASC),\r\n\tCONSTRAINT [fk_orderentry_orderproduct] FOREIGN KEY ([RowNr], [OrderId])\r\n\tREFERENCES [dbo].[OrderEntry] ([RowNr], [OrderId]) ON DELETE CASCADE\r\n);\r\n\r\nCREATE TABLE [dbo].[Customer] (\r\n    [OrderId]   UNIQUEIDENTIFIER    NOT NULL,\r\n    [Firstname] VARCHAR (50)        NOT NULL,\r\n    [Lastname]  VARCHAR (50)        NOT NULL,\r\n    [Email]     VARCHAR (50)        NOT NULL,\r\n    PRIMARY KEY CLUSTERED ([OrderId] ASC),\r\n    CONSTRAINT [fk_order_customer] FOREIGN KEY ([OrderId]) \r\n        REFERENCES [dbo].[Order] ([Id]) ON DELETE CASCADE\r\n);";
            await command.ExecuteNonQueryAsync();
        }
        /// <summary>
        /// drops all tables
        /// </summary>
        /// <returns></returns>
        public async Task DropAsync()
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "DROP TABLE IF EXISTS Customer;\r\nDROP TABLE IF EXISTS OrderProduct;\r\nDROP TABLE IF EXISTS OrderEntry;\r\nDROP TABLE IF EXISTS [Order];\r\nDROP TABLE IF EXISTS ProductDiscountProduct;\r\nDROP TABLE IF EXISTS DiscountFreeProduct;\r\nDROP TABLE IF EXISTS Discount;\r\nDROP TABLE IF EXISTS CartEntry;\r\nDROP TABLE IF EXISTS Cart;\r\nDROP TABLE IF EXISTS Product;\r\nDROP TABLE IF EXISTS Shop;";
            await command.ExecuteNonQueryAsync();
        }

        private async Task InsertAsync()
        {
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();
            using DbCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Shop (Name, AppKey) VALUES \r\n('Herberts Electronic Stuff', 'herbert'), \r\n('ShopB', 'shopB');\r\n\r\ndeclare @shop1id uniqueidentifier, @shop2id uniqueidentifier;\r\nselect @shop1id = id\r\nfrom shop\r\nwhere name = 'Herberts Electronic Stuff';\r\n\r\nselect @shop2id = id\r\nfrom shop\r\nwhere name = 'ShopB';\r\n\r\n\r\nINSERT INTO Product (ShopId, Name, Price, Description, DownloadLink) VALUES\r\n(@shop1id, 'coasters.license' , 2.1, 'coasters software license', 'https://bitorrent/totalylegal/coasters.license/download/7958'),\r\n(@shop1id, 'bookmark.jpg' , 11.6, 'A picture of bookmark (jpg)', 'https://bitorrent/totalylegal/bookmark.jpg/download/47677'),\r\n(@shop1id, 'slipper.exe' , 33.7, 'slipper executable (exe)', 'https://bitorrent/totalylegal/slipper.exe/download/84960'),\r\n(@shop1id, 'blanket.pdf' , 20.58, 'blanket paper (pdf)', 'https://bitorrent/totalylegal/blanket.pdf/download/64972'),\r\n(@shop1id, 'wallet.license' , 23.17, 'wallet software license', 'https://bitorrent/totalylegal/wallet.license/download/59471'),\r\n(@shop1id, 'tooth picks.license' , 30.35, 'tooth picks software license', 'https://bitorrent/totalylegal/tooth_picks.license/download/16130'),\r\n(@shop1id, 'rubber duck.exe' , 10.53, 'rubber duck executable (exe)', 'https://bitorrent/totalylegal/rubber_duck.exe/download/10655'),\r\n(@shop1id, 'piano.jpg' , 46.74, 'A picture of piano (jpg)', 'https://bitorrent/totalylegal/piano.jpg/download/17040'),\r\n(@shop1id, 'shirt.pdf' , 3.18, 'shirt paper (pdf)', 'https://bitorrent/totalylegal/shirt.pdf/download/32785'),\r\n(@shop1id, 'sharpie.png' , 46.5, 'A picture of sharpie (png)', 'https://bitorrent/totalylegal/sharpie.png/download/67092'),\r\n(@shop1id, 'lotion.pdf' , 14.6, 'lotion paper (pdf)', 'https://bitorrent/totalylegal/lotion.pdf/download/21822'),\r\n(@shop1id, 'bracelet.jpg' , 20.24, 'A picture of bracelet (jpg)', 'https://bitorrent/totalylegal/bracelet.jpg/download/22718'),\r\n(@shop1id, 'clamp.exe' , 43.5, 'clamp executable (exe)', 'https://bitorrent/totalylegal/clamp.exe/download/46328'),\r\n(@shop1id, 'doll.license' , 26.59, 'doll software license', 'https://bitorrent/totalylegal/doll.license/download/18878'),\r\n(@shop1id, 'stockings.jpg' , 10.5, 'A picture of stockings (jpg)', 'https://bitorrent/totalylegal/stockings.jpg/download/62952'),\r\n(@shop1id, 'table.jpg' , 44.64, 'A picture of table (jpg)', 'https://bitorrent/totalylegal/table.jpg/download/94810'),\r\n(@shop1id, 'remote.license' , 45.94, 'remote software license', 'https://bitorrent/totalylegal/remote.license/download/47129'),\r\n(@shop1id, 'street lights.license' , 12.54, 'street lights software license', 'https://bitorrent/totalylegal/street_lights.license/download/68379'),\r\n(@shop1id, 'shovel.exe' , 13.63, 'shovel executable (exe)', 'https://bitorrent/totalylegal/shovel.exe/download/76712'),\r\n(@shop1id, 'canvas.png' , 6.32, 'A picture of canvas (png)', 'https://bitorrent/totalylegal/canvas.png/download/70786'),\r\n(@shop1id, 'box.exe' , 40.43, 'box executable (exe)', 'https://bitorrent/totalylegal/box.exe/download/22542'),\r\n(@shop1id, 'spring.png' , 44.6, 'A picture of spring (png)', 'https://bitorrent/totalylegal/spring.png/download/21585'),\r\n(@shop1id, 'conditioner.pdf' , 25.28, 'conditioner paper (pdf)', 'https://bitorrent/totalylegal/conditioner.pdf/download/84619'),\r\n(@shop1id, 'food.png' , 43.37, 'A picture of food (png)', 'https://bitorrent/totalylegal/food.png/download/45885'),\r\n(@shop1id, 'carrots.jpg' , 13.74, 'A picture of carrots (jpg)', 'https://bitorrent/totalylegal/carrots.jpg/download/78086'),\r\n(@shop1id, 'radio.png' , 16.63, 'A picture of radio (png)', 'https://bitorrent/totalylegal/radio.png/download/41037'),\r\n(@shop1id, 'glass.license' , 16.87, 'glass software license', 'https://bitorrent/totalylegal/glass.license/download/58993'),\r\n(@shop1id, 'perfume.license' , 20.86, 'perfume software license', 'https://bitorrent/totalylegal/perfume.license/download/96321'),\r\n(@shop1id, 'mouse pad.license' , 43.18, 'mouse pad software license', 'https://bitorrent/totalylegal/mouse_pad.license/download/52078'),\r\n(@shop1id, 'fork.png' , 29.93, 'A picture of fork (png)', 'https://bitorrent/totalylegal/fork.png/download/11936'),\r\n(@shop1id, 'paint brush.license' , 4.96, 'paint brush software license', 'https://bitorrent/totalylegal/paint_brush.license/download/54893'),\r\n(@shop1id, 'couch.png' , 45.13, 'A picture of couch (png)', 'https://bitorrent/totalylegal/couch.png/download/3477'),\r\n(@shop1id, 'computer.license' , 26.53, 'computer software license', 'https://bitorrent/totalylegal/computer.license/download/80244'),\r\n(@shop1id, 'clock.png' , 48.2, 'A picture of clock (png)', 'https://bitorrent/totalylegal/clock.png/download/58242'),\r\n(@shop1id, 'book.exe' , 44.62, 'book executable (exe)', 'https://bitorrent/totalylegal/book.exe/download/55754'),\r\n(@shop1id, 'fridge.jpg' , 15.15, 'A picture of fridge (jpg)', 'https://bitorrent/totalylegal/fridge.jpg/download/88569'),\r\n(@shop1id, 'ipod.exe' , 27.54, 'ipod executable (exe)', 'https://bitorrent/totalylegal/ipod.exe/download/61764'),\r\n(@shop1id, 'toothbrush.exe' , 4.18, 'toothbrush executable (exe)', 'https://bitorrent/totalylegal/toothbrush.exe/download/50723'),\r\n(@shop1id, 'toe ring.exe' , 43.85, 'toe ring executable (exe)', 'https://bitorrent/totalylegal/toe_ring.exe/download/25746'),\r\n(@shop1id, 'hanger.exe' , 15.62, 'hanger executable (exe)', 'https://bitorrent/totalylegal/hanger.exe/download/95457'),\r\n(@shop1id, 'tomato.jpg' , 46.49, 'A picture of tomato (jpg)', 'https://bitorrent/totalylegal/tomato.jpg/download/78131'),\r\n(@shop1id, 'keyboard.license' , 44.29, 'keyboard software license', 'https://bitorrent/totalylegal/keyboard.license/download/61017'),\r\n(@shop1id, 'eraser.pdf' , 14.28, 'eraser paper (pdf)', 'https://bitorrent/totalylegal/eraser.pdf/download/55336'),\r\n(@shop1id, 'packing peanuts.jpg' , 49.93, 'A picture of packing peanuts (jpg)', 'https://bitorrent/totalylegal/packing_peanuts.jpg/download/80723'),\r\n(@shop1id, 'pen.pdf' , 32.1, 'pen paper (pdf)', 'https://bitorrent/totalylegal/pen.pdf/download/22608'),\r\n(@shop1id, 'blouse.exe' , 44.81, 'blouse executable (exe)', 'https://bitorrent/totalylegal/blouse.exe/download/81038'),\r\n(@shop1id, 'bed.jpg' , 12.21, 'A picture of bed (jpg)', 'https://bitorrent/totalylegal/bed.jpg/download/17228'),\r\n(@shop1id, 'car.png' , 47.35, 'A picture of car (png)', 'https://bitorrent/totalylegal/car.png/download/71278'),\r\n(@shop1id, 'CD.pdf' , 4.46, 'CD paper (pdf)', 'https://bitorrent/totalylegal/CD.pdf/download/55258'),\r\n(@shop1id, 'lamp shade.exe' , 11.69, 'lamp shade executable (exe)', 'https://bitorrent/totalylegal/lamp_shade.exe/download/47401'),\r\n(@shop1id, 'shawl.license' , 35.94, 'shawl software license', 'https://bitorrent/totalylegal/shawl.license/download/95899'),\r\n(@shop1id, 'plate.exe' , 19.11, 'plate executable (exe)', 'https://bitorrent/totalylegal/plate.exe/download/94518'),\r\n(@shop1id, 'thermostat.jpg' , 10.47, 'A picture of thermostat (jpg)', 'https://bitorrent/totalylegal/thermostat.jpg/download/83692'),\r\n(@shop1id, 'candy wrapper.png' , 46.59, 'A picture of candy wrapper (png)', 'https://bitorrent/totalylegal/candy_wrapper.png/download/74518'),\r\n(@shop1id, 'white out.png' , 30.86, 'A picture of white out (png)', 'https://bitorrent/totalylegal/white_out.png/download/92919'),\r\n(@shop1id, 'mop.license' , 38.54, 'mop software license', 'https://bitorrent/totalylegal/mop.license/download/15282'),\r\n(@shop1id, 'house.exe' , 19.58, 'house executable (exe)', 'https://bitorrent/totalylegal/house.exe/download/39279'),\r\n(@shop1id, 'shoes.pdf' , 35.68, 'shoes paper (pdf)', 'https://bitorrent/totalylegal/shoes.pdf/download/18599'),\r\n(@shop1id, 'shampoo.jpg' , 8.23, 'A picture of shampoo (jpg)', 'https://bitorrent/totalylegal/shampoo.jpg/download/44318'),\r\n(@shop1id, 'puddle.jpg' , 34.66, 'A picture of puddle (jpg)', 'https://bitorrent/totalylegal/puddle.jpg/download/11638'),\r\n(@shop1id, 'leg warmers.exe' , 7.83, 'leg warmers executable (exe)', 'https://bitorrent/totalylegal/leg_warmers.exe/download/68000'),\r\n(@shop1id, 'beef.jpg' , 18.88, 'A picture of beef (jpg)', 'https://bitorrent/totalylegal/beef.jpg/download/4817'),\r\n(@shop1id, 'glow stick.png' , 40.67, 'A picture of glow stick (png)', 'https://bitorrent/totalylegal/glow_stick.png/download/54001'),\r\n(@shop1id, 'sand paper.pdf' , 25.97, 'sand paper paper (pdf)', 'https://bitorrent/totalylegal/sand_paper.pdf/download/96964'),\r\n(@shop1id, 'television.jpg' , 24.42, 'A picture of television (jpg)', 'https://bitorrent/totalylegal/television.jpg/download/85766'),\r\n(@shop1id, 'tissue box.license' , 36.52, 'tissue box software license', 'https://bitorrent/totalylegal/tissue_box.license/download/24990'),\r\n(@shop1id, 'tv.license' , 5.76, 'tv software license', 'https://bitorrent/totalylegal/tv.license/download/2197'),\r\n(@shop1id, 'apple.license' , 43.6, 'apple software license', 'https://bitorrent/totalylegal/apple.license/download/65685'),\r\n(@shop1id, 'outlet.license' , 48.67, 'outlet software license', 'https://bitorrent/totalylegal/outlet.license/download/81275'),\r\n(@shop1id, 'desk.license' , 42.59, 'desk software license', 'https://bitorrent/totalylegal/desk.license/download/98578'),\r\n(@shop1id, 'teddies.pdf' , 15.75, 'teddies paper (pdf)', 'https://bitorrent/totalylegal/teddies.pdf/download/79744'),\r\n(@shop1id, 'flag.pdf' , 12.93, 'flag paper (pdf)', 'https://bitorrent/totalylegal/flag.pdf/download/75257'),\r\n(@shop1id, 'bottle.jpg' , 47, 'A picture of bottle (jpg)', 'https://bitorrent/totalylegal/bottle.jpg/download/236'),\r\n(@shop1id, 'bag.jpg' , 21.16, 'A picture of bag (jpg)', 'https://bitorrent/totalylegal/bag.jpg/download/14588'),\r\n(@shop1id, 'soy sauce packet.license' , 9.84, 'soy sauce packet software license', 'https://bitorrent/totalylegal/soy_sauce_packet.license/download/30287'),\r\n(@shop1id, 'fake flowers.pdf' , 14.5, 'fake flowers paper (pdf)', 'https://bitorrent/totalylegal/fake_flowers.pdf/download/76194'),\r\n(@shop1id, 'sun glasses.exe' , 48.15, 'sun glasses executable (exe)', 'https://bitorrent/totalylegal/sun_glasses.exe/download/67354'),\r\n(@shop1id, 'rusty nail.png' , 4.8, 'A picture of rusty nail (png)', 'https://bitorrent/totalylegal/rusty_nail.png/download/82997'),\r\n(@shop1id, 'money.license' , 11.11, 'money software license', 'https://bitorrent/totalylegal/money.license/download/45012'),\r\n(@shop1id, 'video games.exe' , 30.76, 'video games executable (exe)', 'https://bitorrent/totalylegal/video_games.exe/download/67060'),\r\n(@shop1id, 'lace.jpg' , 34.27, 'A picture of lace (jpg)', 'https://bitorrent/totalylegal/lace.jpg/download/61209'),\r\n(@shop1id, 'bowl.png' , 37.32, 'A picture of bowl (png)', 'https://bitorrent/totalylegal/bowl.png/download/5630'),\r\n(@shop1id, 'floor.exe' , 6.55, 'floor executable (exe)', 'https://bitorrent/totalylegal/floor.exe/download/19448'),\r\n(@shop1id, 'bow.exe' , 32.16, 'bow executable (exe)', 'https://bitorrent/totalylegal/bow.exe/download/66132'),\r\n(@shop1id, 'vase.jpg' , 8.33, 'A picture of vase (jpg)', 'https://bitorrent/totalylegal/vase.jpg/download/39080'),\r\n(@shop1id, 'sponge.pdf' , 17.96, 'sponge paper (pdf)', 'https://bitorrent/totalylegal/sponge.pdf/download/98493'),\r\n(@shop1id, 'hair tie.jpg' , 31.3, 'A picture of hair tie (jpg)', 'https://bitorrent/totalylegal/hair_tie.jpg/download/28324'),\r\n(@shop1id, 'buckle.exe' , 27.79, 'buckle executable (exe)', 'https://bitorrent/totalylegal/buckle.exe/download/33251'),\r\n(@shop1id, 'ice cube tray.png' , 8.65, 'A picture of ice cube tray (png)', 'https://bitorrent/totalylegal/ice_cube_tray.png/download/45948'),\r\n(@shop1id, 'washing machine.license' , 31.4, 'washing machine software license', 'https://bitorrent/totalylegal/washing_machine.license/download/20590'),\r\n(@shop1id, 'toothpaste.png' , 15.82, 'A picture of toothpaste (png)', 'https://bitorrent/totalylegal/toothpaste.png/download/85193'),\r\n(@shop1id, 'flowers.jpg' , 49.42, 'A picture of flowers (jpg)', 'https://bitorrent/totalylegal/flowers.jpg/download/78217'),\r\n(@shop1id, 'rug.license' , 42.9, 'rug software license', 'https://bitorrent/totalylegal/rug.license/download/98586'),\r\n(@shop1id, 'milk.license' , 5.71, 'milk software license', 'https://bitorrent/totalylegal/milk.license/download/2116'),\r\n(@shop1id, 'cinder block.license' , 38.65, 'cinder block software license', 'https://bitorrent/totalylegal/cinder_block.license/download/96259'),\r\n(@shop1id, 'boom box.exe' , 32, 'boom box executable (exe)', 'https://bitorrent/totalylegal/boom_box.exe/download/48897'),\r\n(@shop1id, 'tire swing.png' , 15.45, 'A picture of tire swing (png)', 'https://bitorrent/totalylegal/tire_swing.png/download/16568'),\r\n(@shop1id, 'sailboat.png' , 13.5, 'A picture of sailboat (png)', 'https://bitorrent/totalylegal/sailboat.png/download/69699'),\r\n(@shop1id, 'greeting card.license' , 42.46, 'greeting card software license', 'https://bitorrent/totalylegal/greeting_card.license/download/17167'),\r\n(@shop1id, 'newspaper.pdf' , 39.31, 'newspaper paper (pdf)', 'https://bitorrent/totalylegal/newspaper.pdf/download/34189'),\r\n(@shop2id, 'food.exe' , 13.9, 'food executable (exe)', 'https://bitorrent/totalylegal/food.exe/download/42645'),\r\n(@shop2id, 'bed.jpg' , 23.96, 'A picture of bed (jpg)', 'https://bitorrent/totalylegal/bed.jpg/download/64343'),\r\n(@shop2id, 'model car.png' , 18.12, 'A picture of model car (png)', 'https://bitorrent/totalylegal/model_car.png/download/35273'),\r\n(@shop2id, 'keyboard.png' , 22.94, 'A picture of keyboard (png)', 'https://bitorrent/totalylegal/keyboard.png/download/78786'),\r\n(@shop2id, 'canvas.jpg' , 35.79, 'A picture of canvas (jpg)', 'https://bitorrent/totalylegal/canvas.jpg/download/61990'),\r\n(@shop2id, 'puddle.pdf' , 45.71, 'puddle paper (pdf)', 'https://bitorrent/totalylegal/puddle.pdf/download/76927'),\r\n(@shop2id, 'charger.png' , 33.46, 'A picture of charger (png)', 'https://bitorrent/totalylegal/charger.png/download/55856'),\r\n(@shop2id, 'fork.png' , 47.12, 'A picture of fork (png)', 'https://bitorrent/totalylegal/fork.png/download/41966'),\r\n(@shop2id, 'drill press.png' , 2.39, 'A picture of drill press (png)', 'https://bitorrent/totalylegal/drill_press.png/download/4003'),\r\n(@shop2id, 'television.license' , 27.88, 'television software license', 'https://bitorrent/totalylegal/television.license/download/47982'),\r\n(@shop2id, 'glow stick.png' , 33.27, 'A picture of glow stick (png)', 'https://bitorrent/totalylegal/glow_stick.png/download/93485'),\r\n(@shop2id, 'bottle.png' , 43.76, 'A picture of bottle (png)', 'https://bitorrent/totalylegal/bottle.png/download/33335'),\r\n(@shop2id, 'pool stick.png' , 42.4, 'A picture of pool stick (png)', 'https://bitorrent/totalylegal/pool_stick.png/download/90267'),\r\n(@shop2id, 'doll.license' , 26.69, 'doll software license', 'https://bitorrent/totalylegal/doll.license/download/43376'),\r\n(@shop2id, 'white out.jpg' , 40.11, 'A picture of white out (jpg)', 'https://bitorrent/totalylegal/white_out.jpg/download/49918'),\r\n(@shop2id, 'eye liner.license' , 47.75, 'eye liner software license', 'https://bitorrent/totalylegal/eye_liner.license/download/71975'),\r\n(@shop2id, 'headphones.jpg' , 20.11, 'A picture of headphones (jpg)', 'https://bitorrent/totalylegal/headphones.jpg/download/37184'),\r\n(@shop2id, 'rubber duck.jpg' , 22.81, 'A picture of rubber duck (jpg)', 'https://bitorrent/totalylegal/rubber_duck.jpg/download/1301'),\r\n(@shop2id, 'clamp.exe' , 14.84, 'clamp executable (exe)', 'https://bitorrent/totalylegal/clamp.exe/download/8283'),\r\n(@shop2id, 'plastic fork.exe' , 18.74, 'plastic fork executable (exe)', 'https://bitorrent/totalylegal/plastic_fork.exe/download/20555'),\r\n(@shop2id, 'toothbrush.pdf' , 11.45, 'toothbrush paper (pdf)', 'https://bitorrent/totalylegal/toothbrush.pdf/download/23410'),\r\n(@shop2id, 'hair tie.exe' , 41.33, 'hair tie executable (exe)', 'https://bitorrent/totalylegal/hair_tie.exe/download/59696'),\r\n(@shop2id, 'desk.png' , 15.74, 'A picture of desk (png)', 'https://bitorrent/totalylegal/desk.png/download/24451'),\r\n(@shop2id, 'brocolli.pdf' , 17.53, 'brocolli paper (pdf)', 'https://bitorrent/totalylegal/brocolli.pdf/download/74535'),\r\n(@shop2id, 'sticky note.png' , 13.2, 'A picture of sticky note (png)', 'https://bitorrent/totalylegal/sticky_note.png/download/39876'),\r\n(@shop2id, 'tree.license' , 30.66, 'tree software license', 'https://bitorrent/totalylegal/tree.license/download/72291'),\r\n(@shop2id, 'knife.png' , 6.7, 'A picture of knife (png)', 'https://bitorrent/totalylegal/knife.png/download/20547'),\r\n(@shop2id, 'book.pdf' , 33.23, 'book paper (pdf)', 'https://bitorrent/totalylegal/book.pdf/download/99647'),\r\n(@shop2id, 'sidewalk.exe' , 38.13, 'sidewalk executable (exe)', 'https://bitorrent/totalylegal/sidewalk.exe/download/81770'),\r\n(@shop2id, 'carrots.jpg' , 5.93, 'A picture of carrots (jpg)', 'https://bitorrent/totalylegal/carrots.jpg/download/44398'),\r\n(@shop2id, 'shovel.png' , 35.32, 'A picture of shovel (png)', 'https://bitorrent/totalylegal/shovel.png/download/19251'),\r\n(@shop2id, 'shawl.license' , 39.34, 'shawl software license', 'https://bitorrent/totalylegal/shawl.license/download/48748'),\r\n(@shop2id, 'helmet.jpg' , 33.42, 'A picture of helmet (jpg)', 'https://bitorrent/totalylegal/helmet.jpg/download/62245'),\r\n(@shop2id, 'speakers.license' , 9.6, 'speakers software license', 'https://bitorrent/totalylegal/speakers.license/download/44214'),\r\n(@shop2id, 'balloon.pdf' , 10.3, 'balloon paper (pdf)', 'https://bitorrent/totalylegal/balloon.pdf/download/79115'),\r\n(@shop2id, 'sailboat.pdf' , 27.33, 'sailboat paper (pdf)', 'https://bitorrent/totalylegal/sailboat.pdf/download/43941'),\r\n(@shop2id, 'fake flowers.license' , 12.51, 'fake flowers software license', 'https://bitorrent/totalylegal/fake_flowers.license/download/57520'),\r\n(@shop2id, 'water bottle.png' , 24.12, 'A picture of water bottle (png)', 'https://bitorrent/totalylegal/water_bottle.png/download/83911'),\r\n(@shop2id, 'nail clippers.pdf' , 34.31, 'nail clippers paper (pdf)', 'https://bitorrent/totalylegal/nail_clippers.pdf/download/60526'),\r\n(@shop2id, 'sketch pad.exe' , 10.5, 'sketch pad executable (exe)', 'https://bitorrent/totalylegal/sketch_pad.exe/download/91534'),\r\n(@shop2id, 'twezzers.exe' , 8.3, 'twezzers executable (exe)', 'https://bitorrent/totalylegal/twezzers.exe/download/27970'),\r\n(@shop2id, 'twister.pdf' , 49.87, 'twister paper (pdf)', 'https://bitorrent/totalylegal/twister.pdf/download/58353'),\r\n(@shop2id, 'remote.exe' , 36.94, 'remote executable (exe)', 'https://bitorrent/totalylegal/remote.exe/download/33168'),\r\n(@shop2id, 'window.jpg' , 44.5, 'A picture of window (jpg)', 'https://bitorrent/totalylegal/window.jpg/download/65258'),\r\n(@shop2id, 'wallet.png' , 43.17, 'A picture of wallet (png)', 'https://bitorrent/totalylegal/wallet.png/download/55933'),\r\n(@shop2id, 'camera.png' , 32.31, 'A picture of camera (png)', 'https://bitorrent/totalylegal/camera.png/download/14843'),\r\n(@shop2id, 'truck.jpg' , 6.3, 'A picture of truck (jpg)', 'https://bitorrent/totalylegal/truck.jpg/download/85354'),\r\n(@shop2id, 'sofa.jpg' , 35.83, 'A picture of sofa (jpg)', 'https://bitorrent/totalylegal/sofa.jpg/download/71742'),\r\n(@shop2id, 'chalk.exe' , 45.68, 'chalk executable (exe)', 'https://bitorrent/totalylegal/chalk.exe/download/86025'),\r\n(@shop2id, 'bracelet.exe' , 48.8, 'bracelet executable (exe)', 'https://bitorrent/totalylegal/bracelet.exe/download/83833'),\r\n(@shop2id, 'door.png' , 27.4, 'A picture of door (png)', 'https://bitorrent/totalylegal/door.png/download/60526'),\r\n(@shop2id, 'washing machine.jpg' , 9.16, 'A picture of washing machine (jpg)', 'https://bitorrent/totalylegal/washing_machine.jpg/download/40813'),\r\n(@shop2id, 'credit card.exe' , 49.84, 'credit card executable (exe)', 'https://bitorrent/totalylegal/credit_card.exe/download/43922'),\r\n(@shop2id, 'cookie jar.jpg' , 46.64, 'A picture of cookie jar (jpg)', 'https://bitorrent/totalylegal/cookie_jar.jpg/download/65150'),\r\n(@shop2id, 'cinder block.license' , 47.42, 'cinder block software license', 'https://bitorrent/totalylegal/cinder_block.license/download/48982'),\r\n(@shop2id, 'picture frame.jpg' , 27.79, 'A picture of picture frame (jpg)', 'https://bitorrent/totalylegal/picture_frame.jpg/download/71689'),\r\n(@shop2id, 'clay pot.pdf' , 4.57, 'clay pot paper (pdf)', 'https://bitorrent/totalylegal/clay_pot.pdf/download/56330'),\r\n(@shop2id, 'flag.exe' , 43.51, 'flag executable (exe)', 'https://bitorrent/totalylegal/flag.exe/download/36804'),\r\n(@shop2id, 'bread.pdf' , 17.25, 'bread paper (pdf)', 'https://bitorrent/totalylegal/bread.pdf/download/63560'),\r\n(@shop2id, 'spring.pdf' , 9.53, 'spring paper (pdf)', 'https://bitorrent/totalylegal/spring.pdf/download/40618'),\r\n(@shop2id, 'sandal.png' , 24.29, 'A picture of sandal (png)', 'https://bitorrent/totalylegal/sandal.png/download/44991'),\r\n(@shop2id, 'video games.png' , 21.22, 'A picture of video games (png)', 'https://bitorrent/totalylegal/video_games.png/download/55024'),\r\n(@shop2id, 'pillow.pdf' , 13.62, 'pillow paper (pdf)', 'https://bitorrent/totalylegal/pillow.pdf/download/3329'),\r\n(@shop2id, 'rug.png' , 29.17, 'A picture of rug (png)', 'https://bitorrent/totalylegal/rug.png/download/11353'),\r\n(@shop2id, 'ring.pdf' , 27.1, 'ring paper (pdf)', 'https://bitorrent/totalylegal/ring.pdf/download/52444'),\r\n(@shop2id, 'shirt.pdf' , 30.5, 'shirt paper (pdf)', 'https://bitorrent/totalylegal/shirt.pdf/download/54420'),\r\n(@shop2id, 'button.license' , 4.2, 'button software license', 'https://bitorrent/totalylegal/button.license/download/87161'),\r\n(@shop2id, 'air freshener.license' , 42.94, 'air freshener software license', 'https://bitorrent/totalylegal/air_freshener.license/download/73571'),\r\n(@shop2id, 'mirror.exe' , 30.28, 'mirror executable (exe)', 'https://bitorrent/totalylegal/mirror.exe/download/23271'),\r\n(@shop2id, 'soap.pdf' , 44.69, 'soap paper (pdf)', 'https://bitorrent/totalylegal/soap.pdf/download/78927'),\r\n(@shop2id, 'phone.jpg' , 33.51, 'A picture of phone (jpg)', 'https://bitorrent/totalylegal/phone.jpg/download/78622'),\r\n(@shop2id, 'floor.pdf' , 42.42, 'floor paper (pdf)', 'https://bitorrent/totalylegal/floor.pdf/download/37515'),\r\n(@shop2id, 'pants.license' , 45.64, 'pants software license', 'https://bitorrent/totalylegal/pants.license/download/57062'),\r\n(@shop2id, 'face wash.png' , 18.71, 'A picture of face wash (png)', 'https://bitorrent/totalylegal/face_wash.png/download/42683'),\r\n(@shop2id, 'tooth picks.pdf' , 9.41, 'tooth picks paper (pdf)', 'https://bitorrent/totalylegal/tooth_picks.pdf/download/18437'),\r\n(@shop2id, 'table.exe' , 16.6, 'table executable (exe)', 'https://bitorrent/totalylegal/table.exe/download/70791'),\r\n(@shop2id, 'money.exe' , 25.62, 'money executable (exe)', 'https://bitorrent/totalylegal/money.exe/download/66269'),\r\n(@shop2id, 'cell phone.license' , 37.3, 'cell phone software license', 'https://bitorrent/totalylegal/cell_phone.license/download/63817'),\r\n(@shop2id, 'greeting card.jpg' , 24.6, 'A picture of greeting card (jpg)', 'https://bitorrent/totalylegal/greeting_card.jpg/download/20033'),\r\n(@shop2id, 'pen.license' , 2.88, 'pen software license', 'https://bitorrent/totalylegal/pen.license/download/94542'),\r\n(@shop2id, 'clothes.pdf' , 43.76, 'clothes paper (pdf)', 'https://bitorrent/totalylegal/clothes.pdf/download/59816'),\r\n(@shop2id, 'needle.png' , 28.26, 'A picture of needle (png)', 'https://bitorrent/totalylegal/needle.png/download/29757'),\r\n(@shop2id, 'sand paper.exe' , 16.49, 'sand paper executable (exe)', 'https://bitorrent/totalylegal/sand_paper.exe/download/73314'),\r\n(@shop2id, 'chocolate.jpg' , 4.57, 'A picture of chocolate (jpg)', 'https://bitorrent/totalylegal/chocolate.jpg/download/36482'),\r\n(@shop2id, 'toe ring.exe' , 33.88, 'toe ring executable (exe)', 'https://bitorrent/totalylegal/toe_ring.exe/download/99206'),\r\n(@shop2id, 'cup.jpg' , 37.6, 'A picture of cup (jpg)', 'https://bitorrent/totalylegal/cup.jpg/download/25400'),\r\n(@shop2id, 'nail file.jpg' , 15.63, 'A picture of nail file (jpg)', 'https://bitorrent/totalylegal/nail_file.jpg/download/3870'),\r\n(@shop2id, 'rubber band.jpg' , 33.69, 'A picture of rubber band (jpg)', 'https://bitorrent/totalylegal/rubber_band.jpg/download/82582'),\r\n(@shop2id, 'grid paper.exe' , 41.51, 'grid paper executable (exe)', 'https://bitorrent/totalylegal/grid_paper.exe/download/74957'),\r\n(@shop2id, 'socks.png' , 46.66, 'A picture of socks (png)', 'https://bitorrent/totalylegal/socks.png/download/19841'),\r\n(@shop2id, 'chair.license' , 17.81, 'chair software license', 'https://bitorrent/totalylegal/chair.license/download/42505'),\r\n(@shop2id, 'checkbook.exe' , 24.5, 'checkbook executable (exe)', 'https://bitorrent/totalylegal/checkbook.exe/download/83179'),\r\n(@shop2id, 'drawer.license' , 11.13, 'drawer software license', 'https://bitorrent/totalylegal/drawer.license/download/9109'),\r\n(@shop2id, 'bookmark.pdf' , 40.52, 'bookmark paper (pdf)', 'https://bitorrent/totalylegal/bookmark.pdf/download/28544'),\r\n(@shop2id, 'tissue box.pdf' , 41.26, 'tissue box paper (pdf)', 'https://bitorrent/totalylegal/tissue_box.pdf/download/26641'),\r\n(@shop2id, 'towel.png' , 19.28, 'A picture of towel (png)', 'https://bitorrent/totalylegal/towel.png/download/39974'),\r\n(@shop2id, 'rusty nail.pdf' , 13.95, 'rusty nail paper (pdf)', 'https://bitorrent/totalylegal/rusty_nail.pdf/download/56570'),\r\n(@shop2id, 'cork.exe' , 28.73, 'cork executable (exe)', 'https://bitorrent/totalylegal/cork.exe/download/41759'),\r\n(@shop2id, 'shoe lace.png' , 18.31, 'A picture of shoe lace (png)', 'https://bitorrent/totalylegal/shoe_lace.png/download/58550'),\r\n(@shop2id, 'conditioner.license' , 14, 'conditioner software license', 'https://bitorrent/totalylegal/conditioner.license/download/26533');";
            await command.ExecuteNonQueryAsync();
        }

        public async Task InsertSampleImagesAsync()
        { 
            IProductDao productDao = new AdoProductDao(connectionFactory);
            IShopDao shopDao = new AdoShopDao(connectionFactory);
            var shops = await shopDao.FindAllAsync();

            int idx = 0;

            foreach (var shop in shops)
            {
                var products = await productDao.FindAllAsync(shop.Id);
                foreach(var product in products)
                {
                    await using DbConnection connection = await connectionFactory.CreateConnectionAsync();
                    await using DbCommand command = connection.CreateCommand();

                    command.CommandText = "update product set imagenr = @img where id = @pid;";

                    var par = command.CreateParameter();
                    par.ParameterName = "@img";
                    par.Value = ++idx;

                    var pid = command.CreateParameter();
                    pid.ParameterName = "@pid";
                    pid.Value = product.Id;

                    command.Parameters.Add(par);
                    command.Parameters.Add(pid);
                    command.ExecuteNonQuery();

                }
            }
        }

        /// <summary>
        /// inserts the sample data
        /// </summary>
        /// <returns></returns>
        public async Task InsertSampleDataAsync()
        {
            await InsertAsync();
            await using DbConnection connection = await connectionFactory.CreateConnectionAsync();

            IProductDao productDao = new AdoProductDao(connectionFactory);
            IOrderDao orderDao = new AdoOrderDao(connectionFactory);
            ICartDao cartDao = new AdoCartDao(connectionFactory);
            IShopDao shopDao = new AdoShopDao(connectionFactory);
            IDiscountDao discountDao = new AdoDiscountDao(connectionFactory);

            Random random = new(42);
            DateTime endDt = DateTime.Now.AddDays(14);

            var shops = await shopDao.FindAllAsync();

            //2 shops
            for (int j = 0; j < 2; j++)
            {
                var shop = shops.Skip(j).First();
                var customers = (j == 0 ? CustomersRepo.GetCustomersPart1()
                    : CustomersRepo.GetCustomersPart2()).ToArray();
                var products = await productDao.FindAllAsync(shop.Id);

                //orders ~100 customer * 10 orders
                for (int i = 0; i < 1000; i++)
                {
                    //get products
                    int entryCnt = random.Next(1, 7);
                    Dictionary<Guid, ProductAmount> takenProdIds = new();
                    for (int x = 0; x < entryCnt; x++)
                    {
                        //no dublicates
                        Guid prodId;
                        do
                            prodId = products.ElementAt(random.Next(0, 100)).Id;
                        while (takenProdIds.ContainsKey(prodId));

                        takenProdIds.Add(prodId, new ProductAmount(
                            (await productDao.FindByIdAsync(shop.Id, prodId))!,
                            random.Next(1, 5)
                         ));
                    }

                    //build order
                    Order order = new(
                        Guid.Empty,
                        endDt.AddHours(-random.Next(0, 17520)), //about 2 years
                        customers[random.Next(0, 100)],
                        new Cart(Guid.Empty, takenProdIds.Values)
                    );
                    await orderDao.InsertAsync(shop.Id, order);
                }

                //still open carts for 15 customers per shop
                HashSet<int> takenCustIds = new(64);
                for (int i = 0; i <= 15; i++)
                {
                    //1 per cutomer
                    int custId;
                    do
                        custId = random.Next(0, 100);
                    while (takenCustIds.Contains(custId));
                    takenCustIds.Add(custId);


                    int entryCnt = random.Next(1, 8);
                    Cart cart = new(Guid.Empty);
                    HashSet<Guid> takenProdIds = new(64);

                    for (int x = 0; x < entryCnt; x++)
                    {
                        //no dublicates allowed
                        Guid prodId;
                        do
                            prodId = products.ElementAt(random.Next(0, 100)).Id;
                        while (takenProdIds.Contains(prodId));
                        takenProdIds.Add(prodId);

                        cart.Add(new ProductAmount(
                            (await productDao.FindByIdAsync(shop.Id, prodId))!,
                            random.Next(1, 5)
                        ));
                    }
                    await cartDao.InsertAsync(shop.Id, cart);
                }

                // insert sample discount

                var topsellers = await shopDao.EvaluateTopsellersAsync(shops.ElementAt(j).Id, 10, new DateTime(2000, 1, 1), DateTime.Now);
                await discountDao.InsertAsync(shops.ElementAt(j).Id, new Discount(
                    Guid.Empty,
                    OffType.Percentual, 0.2m,
                    "20% off at the specified products",
                    "20% off",
                    MinType.ProductCount, 1, false,
                    DateTime.Now.AddHours(-3),
                    DateTime.Now.AddYears(1), null,
                    new Guid[] { topsellers.ElementAt(0).Product.Id, topsellers.ElementAt(3).Product.Id }
                ));

            }
        }
    }
}

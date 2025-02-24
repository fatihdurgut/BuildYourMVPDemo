-- Insert Categories
INSERT INTO [dbo].[Categories] ([Name], [Description])
VALUES 
    ('Tents', 'High-quality camping tents for all your outdoor adventures'),
    ('Backpacks', 'Durable backpacks for hiking and camping'),
    ('Clothing', 'Essential outdoor clothing and gear');

-- Insert Products
-- Tents
INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [CategoryId], [StockQuantity])
VALUES
    ('TrailMaster X4 Tent', 'Four-person camping tent with weather-resistant design', 200.00, '/images/tent1.png', 1, 10),
    ('Alpine Explorer Tent', 'Lightweight two-person tent for mountain camping', 300.00, '/images/tent2.png', 1, 15),
    ('Sky View 2-Person Tent', 'Tent with transparent roof panel for stargazing', 380.00, '/images/tent3.png', 1, 8);

-- Backpacks
INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [CategoryId], [StockQuantity])
VALUES
    ('Adventurer Pro Backpack', 'Large capacity hiking backpack with multiple compartments', 60.00, '/images/backpack1.png', 2, 20),
    ('Summit Climber Backpack', 'Professional climbing backpack with gear attachments', 70.00, '/images/backpack2.png', 2, 25),
    ('TrailLite DayPack', 'Lightweight daypack for short hiking trips', 40.00, '/images/backpack3.png', 2, 30);

-- Clothing
INSERT INTO [dbo].[Products] ([Name], [Description], [Price], [ImageUrl], [CategoryId], [StockQuantity])
VALUES
    ('Summit Breeze Jacket', 'Waterproof and breathable hiking jacket', 60.00, '/images/jacket.png', 3, 15),
    ('Trail Blaze Pants', 'Durable hiking pants with convertible legs', 80.00, '/images/pants.png', 3, 20),
    ('TREKSTAR Boots', 'All-terrain hiking boots with ankle support', 120.00, '/images/boots.png', 3, 12);
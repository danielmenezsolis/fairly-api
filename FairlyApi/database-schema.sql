-- 1. Create the schema, avoiding an error if it already exists.
CREATE SCHEMA IF NOT EXISTS fairly;
-- 2. Set the search path to Fairly to simplify table creation commands.
SET search_path TO fairly, public;
-- 3. Create Users table
CREATE TABLE fairly.Users (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    profile_picture_url VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);
-- 4. Create Groups table
CREATE TABLE fairly.Groups (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    creator_id UUID NOT NULL REFERENCES fairly.Users(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);
-- 5. Create GroupMembers table (N:M relationship)
CREATE TABLE fairly.GroupMembers (
    user_id UUID REFERENCES fairly.Users(id),
    group_id INT REFERENCES fairly.Groups(id),
    PRIMARY KEY (user_id, group_id)
);
-- 6. Create Expenses table
CREATE TABLE fairly.Expenses (
    id SERIAL PRIMARY KEY,
    group_id INT NOT NULL REFERENCES fairly.Groups(id),
    payer_id UUID NOT NULL REFERENCES fairly.Users(id),
    total_amount NUMERIC(10, 2) NOT NULL CHECK (total_amount > 0),
    description VARCHAR(255) NOT NULL,
    expense_date DATE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now()
);
-- 7. Create ExpenseParticipants table (Split breakdown)
CREATE TABLE fairly.ExpenseParticipants (
    expense_id INT REFERENCES fairly.Expenses(id),
    user_id UUID REFERENCES fairly.Users(id),
    amount_owed NUMERIC(10, 2) NOT NULL,
    PRIMARY KEY (expense_id, user_id)
);
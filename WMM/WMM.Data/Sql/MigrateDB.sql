BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS `Goals` (
	`Id`	BLOB NOT NULL,
	`Name`	TEXT NOT NULL UNIQUE,
	`Description`	TEXT,
	`Criteria`	TEXT NOT NULL,
	`Limit`	NUMERIC NOT NULL,
	PRIMARY KEY(`Id`)
);
COMMIT;
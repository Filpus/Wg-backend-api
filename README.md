### generate sql seeder

```bash
psql -U username -d databse_name -f .\Wg-backend-api\Migrationswg-init-db-seeder.sql
```
Example:
```bash
psql -U postgres -d wg -f .\Wg-backend-api\Migrations\wg-init-db-seeder.sql
```
game-schema-init.sql file, is for creating the schema of game_1, without global schema, or any data.

Reseting db with .bat file
```bash
./reset-db.bat
```

### before push changes

Format main project
```bash
./format.bat
```

### Create sql backup
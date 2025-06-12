### generate sql seeder

```bash
psql -U username -d databse_name -f wg-init-db-seeder.sql
```
Example :
```bash
psql -U postgres -d wg -f wg-init-db-seeder.sql
```
After execute this command change name of default_schema to game_1

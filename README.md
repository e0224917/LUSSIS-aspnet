# LUSSIS-aspnet

## Requirement
1. Visual Studio 2017 (latest update)
2. Microsoft SQL Server 2017
3. Crystal report for VS ([link](http://www.crystalreports.com/crvs/confirm/))

## Database backup
Backup file and SQl script location: inside SQL folder

## Login account
Every employee has account, with their email as username, password is "password". Roles is based on job title.

| Email address                      | Password | Job title  |
| ---------------------------------- |:--------:| -----------|
| esther_tan@logicuniversity.edu     | password | Clerk      |
| jamya_burton@logicuniversity.edu   | password | Supervisor |
| kadin_ashley@logicuniversity.edu   | password | Manager    |
| liam_colon@logicuniversity.edu     | password | Staff      |
| dillan_rosales@logicuniversity.edu | password | Rep        |
| ezra_pound@logicuniversity.edu     | password | Head       |

## Set up SSL for publishing to IIS (optional):
1. Create self-signed SSL certificate:
- Open IIS Manager, go to the local computer node
- Double click Server Certificates
- On right panel, click on Create Self-Signed Certificate
- Input name and click Ok

2. Enable SSL
- On IIS Manager, choose Sites node
- Choose to Default Web Site
- On right panel, click on Bindings...
- Click add, on Type choose https, on SSL certificate choose the newly created certificate, click Ok

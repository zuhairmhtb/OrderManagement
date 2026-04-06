# Order Management Application

This is a .Net 10 based web application developed for interview at Talvette. The application contains an order management system developed as a microservice application using EntityFramework.

### Application Overview

The application contains different products. Customers can sign up to the application and place orders to purchase different products.

### Business requirements

1. **Product:** Products get added by default to the database (seeding). The products can be viewed by the API but it cannot be added or updated.

2. **Customer:** Customers can register within the application to purchase different products by placing orders. The application during sign up uses customer's email and password to create an account. The customer can later manage the profile by adding their First name, Last name, Address, and Contact information.

3. **Order:** Orders can be placed by customers to purchase different products. When placing order, the customer needs to provide the list of different products that will be purchased, provides the delivery, and billing address. Upon placing an order, the customer receives a success message and can view the order details. The customer can track the order status using the order number.

4. **Order Filtering:** Admin users can view all orders and filter different orders based on: Date range, order status, and customer email.

### Technical Requirements

1. **Logging:** The application uses default ILogger for logging. Currently, the logs will be displayed through stdout. However, it can be modified later to store the logs to a database like ElasticSearch.

2. **Database Management:** MSSQL will be used as the database of choice. EntityFramework will be used as the ORM to handle communication with the database.

3. **Databae Indexing:** Proper indexing strategy needs to be applied to optimize the performance of database query and data retrieval. Users will be able to query orders based on the indexed fields.

4. **CRUD Operation:** Currently, the application contains a Web API to manage orders and customers. Swagger UI will be used to communicate with the API application.

5. **Workers to Write Data:** All write operations to the database will be managed by a service worker (console application) created for the Order Management System. The API application will send commands via MassTransit to communicate with the service worker. This is done to ensure scalability of the application and delegate the query and retrieval related task to the API application.

6. **JWT Authentication and Authorization:** JWT authentication will be used to handle customer login. Currently, the following roles are available for authorization: admin, customer. 'admin' users will be able to view all orders. However, users with 'customer' role will only be able to view their own orders.

7. **Sensitive Data Handling:** Sensitive data like password will be hashed and stored in the database to ensure security and GDPR compliance.

8. **Unit Testing:** Sample unit tests will be written in Xunit for the services of the WebApi application to demonstrate the implementation of unit testing. It will include dependency mocking.

9. **Containerization:** The applications (worker and API) will be containerized using Docker to ensure portability and ease of deployment.

### Descriptive Answers

1. **Audit logging and data retention policies:** Explain how you would implement audit logging and data retention policies.

2. **Integrating test to CI/CD pipeline:** Describe how you would integrate tests into CI/CD pipeline.


### 1. Audit Logging and data retention policies

Currently, the logs of the application are being collected by the standard logging packaging of .Net. The logs are being displayed in the standard output.

However, in a production grade system we need to collect logs from different servers to a centralized storage like NoSQL database or cloud storage buckets.

Since the log contain traces of the application, it is very useful in debugging a production application without requiring to debug the code within a production server. Detailed logs help trace a request starting from when it is first received by the controller up to the data layer.

### 1.1 Nlog for collecting Logs
To upgrade, the logging of the system I would prefer NLog package since it provides a declarative way of configuring how logs will be collected, stored, and transmitted. It provides the feature to display the logs in standard output, store them in file, and even store them in a relational or non-relational database. We can update the NLog's configuration file and make the required changes. It also provides the benifit of configuring log rotations to manage the increasing size of the log.

### 1.2 NoSQL Database
For production application, I would prefer transferring the log over to a NoSQL database like Elasticsearch. The logs can then be analyzed and searched when performing diagnosis of the system. The easiest approach would be to have a client-facing Elasticsearch database. This is usually quite enough in small or medium sized system. However, if the size of the system starts increasing, the frequency of the logs increase. In that case, we could use an ETL pipeline like a client-facing Apache Kafka connected to the ElasticSearch database to collect and store the logs.

### 1.3 Log Retention policy

When dealing with a large number of logs, we need to implement different policies to periodically transfer old logs to either a cold storage or remove them. This needs to be performed for managing cost and efficiency of the system. In such cases, it is very common to divide the logs into 3 different categories based the duration of their last modified timestamp and access frequency - hot (new logs that may require analysis), warm (logs that have either been analyzed or is unlikely to be analyzed frequently), and cold (logs that are kept for audit purpose and is unlikely to be used for analysis of the system). Next, we can configure different cloud storage buckets like Amazon S3 Glacier to store the warm and cold logs. A service like a console application can manage the transfer if not supported by the cloud infrastructure. Usually, logs are transferred from one stage to another every 90 days but it mostly depends on the business needs.

### 1.4 Visualization

A dashboard like Kibana or Grafana can be used to view and analyze the logs if they are stored in ElasticSearch.

### 2. Integrating test CI/CD pipeline

We can use different CI/CD pipelines like Azure DevOps and Github Actions to manage the deployment. Since, the whole application is containerized, the application can run on both Windows and Linux machines. The application can be therefore easily deployed with a CI/CD pipeline.

Since this codebase has been uploaded to Github, I will choose **Github Actions** as the deployment pipeline of the application.

### 2.1 YML Template

The first approach would be to create a pipeline using YML configuration. We can create a .github/workflows folder and add the yaml configuration to it. We will perform the following tasks in the pipeline:

1. Pull the recent changes of the Github source code.

2. Add the proper environment variables to the Github runner.

3. Generate the apsettings file with the proper configuration values.

4. Build the docker image from the Dockerfile

5. Push the Image to a Docker image repository like Docker Hub.

6. Access the deployment server using ssh

7. Pull the docker image from the repository and create the related docker service containers from the docker compose configuration.

8. Deploy each service (web and worker) individually to different server or the same one.

### 2.2 Github Secrets and Variables

I will store the deployment and application related secrets in Github Secrets Vault. The deployment pipeline can easily access the secrets and generate the required environment file and appsettings file. The sensitive values will be stored in 'Secrets' and the rest would be stored in 'Variables'.

If the secrets are already stored in a cloud storage like AWS Secret Manager, we can use access tokens to fetch the secrets and inject it to the docker container at runtime. The access tokens can then be stored in Github secrets and added during deployment.

### 2.3 SSH To Access Deployment Server

For this application, I would use SSH to access the deployment servers and run bash scripts (assuming debian based servers) to run the docker container. However, in a production grade application we use multiple instances to load balance an application. In such scenario, we can configure the YML template to deploy the application to multiple server.

However, when the system needs to be deployed to multiple servers, it is easier to use Azure DevOps with .Net applications since we can connect different servers to Devops and easily configure the pipeline to manage deploying same codebase sequentially to different servers.

### 2.4 Managing Environments

Usually, the codebase is not directly deployed to a production server since it may contain bugs and cause unwanted side effects to the production system. Therefore, usually different approaches like blue-green deployment is used where the application is first deployed to a staging environment where it is tested thoroughly and on approval deployed to production environment. This reduces the risk of introducing bugs in the system which might affect the users.
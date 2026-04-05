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

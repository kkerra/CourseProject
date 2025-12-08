CREATE DATABASE  IF NOT EXISTS `course_project` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `course_project`;
-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: course_project
-- ------------------------------------------------------
-- Server version	8.0.44

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `call`
--

DROP TABLE IF EXISTS `call`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `call` (
  `call_id` int NOT NULL AUTO_INCREMENT,
  `call_datetime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `duration` int NOT NULL,
  `result` varchar(45) NOT NULL,
  `comment` varchar(100) DEFAULT NULL,
  `employee_id` int DEFAULT NULL,
  `client_id` int DEFAULT NULL,
  PRIMARY KEY (`call_id`),
  KEY `employee_id` (`employee_id`),
  KEY `client_id` (`client_id`),
  CONSTRAINT `call_ibfk_1` FOREIGN KEY (`employee_id`) REFERENCES `employee` (`employee_id`),
  CONSTRAINT `call_ibfk_2` FOREIGN KEY (`client_id`) REFERENCES `client` (`client_id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `call`
--

LOCK TABLES `call` WRITE;
/*!40000 ALTER TABLE `call` DISABLE KEYS */;
INSERT INTO `call` VALUES (1,'2024-01-15 10:30:00',245,'Успешно','Клиент интересуется подключением интернета и ТВ',1,1),(2,'2024-01-15 11:15:00',180,'Успешно','Консультация по тарифам интернета',2,2),(3,'2024-01-15 14:20:00',320,'Отказ','Клиент не отвечает',1,3),(4,'2024-01-15 16:45:00',560,'Успешно','Подключение пакета услуг: интернет + ТВ + роутер',2,4),(5,'2024-01-16 09:10:00',190,'Успешно','Запрос на замену роутера на Wi-Fi 6',1,5),(6,'2024-01-16 12:30:00',275,'Успешно','Консультация по ТВ-приставкам',2,1),(7,'2024-01-16 15:40:00',420,'Успешно','Подключение дополнительной ТВ-приставки',1,2),(8,'2025-12-07 16:55:34',30,'Успешно','1',1,8),(9,'2025-12-07 17:01:59',30,'Успешно','1',1,8),(10,'2025-12-07 17:13:10',30,'Перенос','1',1,8),(11,'2024-01-17 10:00:00',310,'Успешно','Подключение интернета 500 Мбит/с и облачного хранилища',3,6),(12,'2024-01-17 13:25:00',180,'Отказ','Клиент занят, перезвонить после 18:00',1,1),(13,'2024-01-17 15:40:00',425,'Успешно','Переход с тарифа 100 на 300 Мбит/с',4,1),(14,'2024-01-18 11:15:00',520,'Успешно','Подключение полного пакета: интернет+ТВ+моб.связь+антивирус',3,8),(15,'2024-01-18 14:30:00',195,'Успешно','Консультация по облачному хранилищу',4,5),(16,'2024-01-18 16:45:00',340,'Успешно','Жалоба на скорость интернета, проведена диагностика',1,2),(17,'2024-01-19 09:20:00',270,'Успешно','Подключение антивируса к существующим услугам',3,9),(18,'2025-12-07 19:49:58',30,'Успешно','1',4,8);
/*!40000 ALTER TABLE `call` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `call_service`
--

DROP TABLE IF EXISTS `call_service`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `call_service` (
  `call_id` int NOT NULL,
  `service_id` int NOT NULL,
  PRIMARY KEY (`call_id`,`service_id`),
  KEY `service_id` (`service_id`),
  CONSTRAINT `call_service_ibfk_1` FOREIGN KEY (`call_id`) REFERENCES `call` (`call_id`),
  CONSTRAINT `call_service_ibfk_2` FOREIGN KEY (`service_id`) REFERENCES `service` (`service_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `call_service`
--

LOCK TABLES `call_service` WRITE;
/*!40000 ALTER TABLE `call_service` DISABLE KEYS */;
INSERT INTO `call_service` VALUES (1,1),(4,1),(8,1),(10,1),(11,1),(18,1),(2,2),(9,2),(10,2),(1,3),(13,3),(4,4),(11,4),(1,5),(2,5),(4,6),(5,6),(10,6),(4,7),(6,7),(6,8),(7,8),(8,9),(11,10),(11,11),(13,11),(14,11),(8,12),(12,12);
/*!40000 ALTER TABLE `call_service` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `client`
--

DROP TABLE IF EXISTS `client`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `client` (
  `client_id` int NOT NULL AUTO_INCREMENT,
  `surname` varchar(45) NOT NULL,
  `name` varchar(45) NOT NULL,
  `patronymic` varchar(45) DEFAULT NULL,
  `address` varchar(150) DEFAULT NULL,
  `phone_number` varchar(20) NOT NULL,
  `interaction_status` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`client_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `client`
--

LOCK TABLES `client` WRITE;
/*!40000 ALTER TABLE `client` DISABLE KEYS */;
INSERT INTO `client` VALUES (1,'Смирнов','Иван','Александрович','г. Москва, ул. Ленина, д. 15, кв. 42','+79161234567','Активный'),(2,'Кузнецова','Елена','Викторовна','г. Москва, пр-т Мира, д. 28, кв. 15','+79167654321','Активный'),(3,'Попов','Сергей','Николаевич','г. Москва, ул. Пушкина, д. 5, кв. 78','+79162345678','Потенциальный'),(4,'Васильева','Анна','Дмитриевна','г. Москва, ул. Гагарина, д. 33, кв. 24','+79168765432','Неактивный'),(5,'Морозов','Павел','Олегович','г. Москва, ул. Садовая, д. 12, кв. 56','+79163456789','Активный'),(6,'asd','sadsd','asdsd','qwqwqweeqwewq','12345678','Новый'),(7,'йцуйцу','цйуцйуйцу','цуйцуцйу','йцуйцу','йцуйцуйц','Новый'),(8,'1','1','1','1','1','Новый'),(9,'Орлов','Михаил','Борисович','г. Москва, ул. Тверская, д. 10, кв. 33','+79165557788','активный'),(10,'Жукова','Татьяна','Ивановна','г. Москва, ул. Чехова, д. 7, кв. 19','+79160011223','потенциальный'),(11,'Лебедев','Антон','Павлович','г. Москва, Ленинский пр-т, д. 45, кв. 81','+79164446699','неактивный'),(12,'Семенова','Виктория','Олеговна','г. Москва, ул. Кутузова, д. 22, кв. 64','+79168887766','активный'),(13,'Никитин','Роман','Витальевич','г. Москва, ул. Цветной бульвар, д. 15, кв. 7','+79163332211','активный');
/*!40000 ALTER TABLE `client` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employee`
--

DROP TABLE IF EXISTS `employee`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee` (
  `employee_id` int NOT NULL AUTO_INCREMENT,
  `surname` varchar(45) NOT NULL,
  `name` varchar(45) NOT NULL,
  `patronymic` varchar(45) DEFAULT NULL,
  `login` varchar(20) NOT NULL,
  `password` varchar(20) NOT NULL,
  `email` varchar(50) DEFAULT NULL,
  `is_active` bit(1) NOT NULL DEFAULT b'1',
  `role_id` int DEFAULT NULL,
  PRIMARY KEY (`employee_id`),
  UNIQUE KEY `login` (`login`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `employee_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `role` (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee`
--

LOCK TABLES `employee` WRITE;
/*!40000 ALTER TABLE `employee` DISABLE KEYS */;
INSERT INTO `employee` VALUES (1,'Иванов','Алексей','Петрович','ivanov2','pass123','ivanov@company.ru',_binary '',1),(2,'Петрова','Мария','Сергеевна','petrova','pass456','petrova@company.ru',_binary '',1),(3,'Сидоров','Дмитрий','Игоревич','sidorov','pass789','sidorov@company.ru',_binary '',1),(4,'Козлова','Ольга','Владимировна','kozlova','pass012','kozlova@company.ru',_binary '',2),(5,'abc','abc','abc','abc1','123','abc1@company.com',_binary '\0',1),(6,'123','123','123','123','123','231',_binary '\0',1),(8,'Новиков','Андрей','Валерьевич','novikov_a','qwerty111','novikov@company.ru',_binary '\0',1),(10,'Громов','Игорь','Сергеевич','gromov_i','superpass','gromov@company.ru',_binary '',2),(11,'123','123','123','1233','111111','123',_binary '',1);
/*!40000 ALTER TABLE `employee` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role`
--

DROP TABLE IF EXISTS `role`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role` (
  `role_id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(45) NOT NULL,
  PRIMARY KEY (`role_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role`
--

LOCK TABLES `role` WRITE;
/*!40000 ALTER TABLE `role` DISABLE KEYS */;
INSERT INTO `role` VALUES (1,'Оператор'),(2,'Супервайзер');
/*!40000 ALTER TABLE `role` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `service`
--

DROP TABLE IF EXISTS `service`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `service` (
  `service_id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(50) NOT NULL,
  `description` varchar(200) DEFAULT NULL,
  `price` decimal(4,0) NOT NULL,
  `category` varchar(45) NOT NULL,
  PRIMARY KEY (`service_id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `service`
--

LOCK TABLES `service` WRITE;
/*!40000 ALTER TABLE `service` DISABLE KEYS */;
INSERT INTO `service` VALUES (1,'Домашний интернет 100 Мбит/с','Высокоскоростной домашний интернет 100 Мбит/с',500,'Интернет'),(2,'Домашний интернет 300 Мбит/с','Высокоскоростной домашний интернет 300 Мбит/с',700,'Интернет'),(3,'Цифровое ТВ Базовый','Базовый пакет цифрового телевидения',300,'Телевидение'),(4,'Цифровое ТВ Премиум','Расширенный пакет цифрового телевидения',500,'Телевидение'),(5,'Роутер Wi-Fi 5','Аренда роутера стандарта Wi-Fi 5 (802.11ac)',50,'Оборудование'),(6,'Роутер Wi-Fi 6','Аренда роутера стандарта Wi-Fi 6 (802.11ax)',100,'Оборудование'),(7,'ТВ-приставка Стандарт','ТВ-приставка для цифрового телевидения',75,'Оборудование'),(8,'ТВ-приставка 4K','ТВ-приставка с поддержкой 4K разрешения',120,'Оборудование'),(9,'Домашний интернет 500 Мбит/с','Ультраскоростной домашний интернет 500 Мбит/с',900,'Интернет'),(10,'Мобильная связь + интернет','Пакет мобильной связи с интернетом 30 ГБ',400,'Мобильная связь'),(11,'Антивирусная защита','Защита устройства от вирусов и вредоносных программ',150,'Дополнительные услуги'),(12,'Облачное хранилище 100 ГБ','Персональное облачное хранилище для файлов',200,'Дополнительные услуги');
/*!40000 ALTER TABLE `service` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-12-07 22:22:50

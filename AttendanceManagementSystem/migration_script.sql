CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "Email" TEXT NULL,
    "Password" TEXT NULL,
    "Role" TEXT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241219120916_InitialCreate', '9.0.0');

ALTER TABLE "Users" ADD "ResetToken" TEXT NULL;

ALTER TABLE "Users" ADD "ResetTokenExpiry" TEXT NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241220090433_AddResetTokenToUser', '9.0.0');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241220140737_AddCourseTable', '9.0.0');

CREATE TABLE "Courses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Courses" PRIMARY KEY AUTOINCREMENT,
    "CourseName" TEXT NULL,
    "CourseCode" TEXT NULL,
    "TeacherName" TEXT NULL,
    "CreditHours" INTEGER NOT NULL,
    "CreatedBy" TEXT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241220141825_AddCourseTableV2', '9.0.0');

CREATE TABLE "Subjects" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Subjects" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "Code" TEXT NULL
);

CREATE TABLE "Enrollments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Enrollments" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "SubjectId" INTEGER NOT NULL,
    CONSTRAINT "FK_Enrollments_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES "Subjects" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrollments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Enrollments_SubjectId" ON "Enrollments" ("SubjectId");

CREATE INDEX "IX_Enrollments_UserId" ON "Enrollments" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241221074258_AddSubjectsAndEnrollments', '9.0.0');

DROP TABLE "Subjects";

DROP INDEX "IX_Enrollments_SubjectId";

ALTER TABLE "Enrollments" ADD "CoursesId" INTEGER NULL;

CREATE INDEX "IX_Enrollments_CoursesId" ON "Enrollments" ("CoursesId");

CREATE TABLE "ef_temp_Enrollments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Enrollments" PRIMARY KEY AUTOINCREMENT,
    "CoursesId" INTEGER NULL,
    "SubjectId" INTEGER NOT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "FK_Enrollments_Courses_CoursesId" FOREIGN KEY ("CoursesId") REFERENCES "Courses" ("Id"),
    CONSTRAINT "FK_Enrollments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Enrollments" ("Id", "CoursesId", "SubjectId", "UserId")
SELECT "Id", "CoursesId", "SubjectId", "UserId"
FROM "Enrollments";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Enrollments";

ALTER TABLE "ef_temp_Enrollments" RENAME TO "Enrollments";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Enrollments_CoursesId" ON "Enrollments" ("CoursesId");

CREATE INDEX "IX_Enrollments_UserId" ON "Enrollments" ("UserId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241221141540_AddEnrollments', '9.0.0');

BEGIN TRANSACTION;
DROP INDEX "IX_Enrollments_CoursesId";

ALTER TABLE "Enrollments" RENAME COLUMN "UserId" TO "StudentId";

ALTER TABLE "Enrollments" RENAME COLUMN "SubjectId" TO "CourseId";

DROP INDEX "IX_Enrollments_UserId";

CREATE INDEX "IX_Enrollments_StudentId" ON "Enrollments" ("StudentId");

ALTER TABLE "Enrollments" ADD "StudentCode" TEXT NULL;

ALTER TABLE "Enrollments" ADD "StudentName" TEXT NULL;

CREATE INDEX "IX_Enrollments_CourseId" ON "Enrollments" ("CourseId");

CREATE TABLE "ef_temp_Enrollments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Enrollments" PRIMARY KEY AUTOINCREMENT,
    "CourseId" INTEGER NOT NULL,
    "StudentCode" TEXT NULL,
    "StudentId" INTEGER NOT NULL,
    "StudentName" TEXT NULL,
    CONSTRAINT "FK_Enrollments_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Enrollments_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Enrollments" ("Id", "CourseId", "StudentCode", "StudentId", "StudentName")
SELECT "Id", "CourseId", "StudentCode", "StudentId", "StudentName"
FROM "Enrollments";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Enrollments";

ALTER TABLE "ef_temp_Enrollments" RENAME TO "Enrollments";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Enrollments_CourseId" ON "Enrollments" ("CourseId");

CREATE INDEX "IX_Enrollments_StudentId" ON "Enrollments" ("StudentId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241221142819_V2AddEnrollments', '9.0.0');

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241221145556_V3AddEnrollments', '9.0.0');

CREATE TABLE "Attendances" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Attendances" PRIMARY KEY AUTOINCREMENT,
    "StudentId" INTEGER NOT NULL,
    "CourseId" INTEGER NOT NULL,
    "Date" TEXT NOT NULL,
    "Status" TEXT NULL,
    CONSTRAINT "FK_Attendances_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Attendances_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Attendances_CourseId" ON "Attendances" ("CourseId");

CREATE INDEX "IX_Attendances_StudentId" ON "Attendances" ("StudentId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20241221160454_AddAttendanceTable', '9.0.0');

COMMIT;


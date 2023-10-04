<?php // deleteobject.php

const ERROR_MISSING_FIELDS = "7";
const ERROR_NO_ROW_DELETED = "8";
const ERROR_EXECUTION_FAILED = "9";

if (isset($_POST["id"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php'; // grants pdo and conn
        
        $id = $_POST["id"];
        
        $sql = 'DELETE FROM `the_builded` WHERE `id`=:id';
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':id', $id, PDO::PARAM_INT);
        $stmt->execute();
        
        if ($stmt->rowCount() > 0) {
            echo "0";
        } else {
            echo ERROR_NO_ROW_DELETED;
        }
        
    } catch (PDOException $e) {
        error_log("Database error: " . $e->getMessage()); // Log the error
        echo ERROR_EXECUTION_FAILED;
    }

    exit();
} else {
    echo ERROR_MISSING_FIELDS;
    exit();
}

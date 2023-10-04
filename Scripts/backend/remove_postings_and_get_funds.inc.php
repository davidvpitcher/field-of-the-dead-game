<?php // remove_postings_and_get_funds.inc.php

const ERROR_MISSING_FIELDS = "7";
const ERROR_NOT_FOUND = "1";
const ERROR_EXECUTION_FAILED = "2";

if (isset($_POST["code"]) && isset($_POST["playername"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php';

        $username = $_POST["playername"];
        
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        $pdo->beginTransaction();

        $stmt = $pdo->prepare("SELECT SUM(price * quantity) AS total_funds FROM market_sales WHERE seller = :username;");
        $stmt->bindParam(':username', $username, PDO::PARAM_STR);
        $stmt->execute();
        
        $result = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($result) {
            $total_funds = $result["total_funds"];
            
            $stmt = $pdo->prepare("DELETE FROM market_sales WHERE seller = :username;");
            $stmt->bindParam(':username', $username, PDO::PARAM_STR);
            $successful = $stmt->execute();
            
            if (!$successful) {
                throw new Exception("Failed to delete market sales");
            }
            
            $pdo->commit();
            echo "0" . $total_funds;
        } else {
            echo ERROR_NOT_FOUND;
        }
    } catch (PDOException $e) {
        $pdo->rollback();
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    } catch (Exception $e) {
        $pdo->rollback();
        error_log("General error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }

    exit();
} else {
    echo ERROR_MISSING_FIELDS;
    exit();
}

<?php // marketposting.inc.php

const ERROR_INVALID_ITEM_NAME = "7";
const ERROR_EXECUTION_FAILED = "1";

if (isset($_POST["item_name"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php';
        
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
        
        $itemname = $_POST["item_name"];
        $itemquantity = $_POST["item_quantity"];
        $itemseller = $_POST["item_seller"];
        $itemprice = $_POST["item_price"];
        $itemlevel = $_POST["item_level"];
        
        $sql = 'INSERT INTO `market_items` SET `name` = :itemname, `quantity` = :itemquantity, `seller` = :itemseller, `price` = :itemprice, `level` = :itemlevel';
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':itemname', $itemname, PDO::PARAM_STR);
        $stmt->bindParam(':itemquantity', $itemquantity, PDO::PARAM_INT);
        $stmt->bindParam(':itemseller', $itemseller, PDO::PARAM_STR);
        $stmt->bindParam(':itemprice', $itemprice, PDO::PARAM_INT);
        $stmt->bindParam(':itemlevel', $itemlevel, PDO::PARAM_INT);
        $stmt->execute();
        
        echo "0";
        
    } catch (PDOException $e) {
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    } catch (Exception $e) {
        error_log("General error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }
} else {
    echo ERROR_INVALID_ITEM_NAME;
}

exit();
?>

<?php // storeobject.php

$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["type"])) {
    echo "7: uh oh";
    exit();
}

// Sanitize and Validate Inputs
$type = trim($_POST["type"]);
$posX = trim($_POST["posX"]);
$posY = trim($_POST["posY"]);
$posZ = trim($_POST["posZ"]);
$rotX = trim($_POST["rotX"]);
$rotY = trim($_POST["rotY"]);
$rotZ = trim($_POST["rotZ"]);
$rotQ = trim($_POST["rotQ"]);
$hp = trim($_POST["hp"]);

if (empty($type) || !is_numeric($posX) || !is_numeric($posY) || !is_numeric($posZ) || !is_numeric($rotX) || !is_numeric($rotY) || !is_numeric($rotZ) || !is_numeric($rotQ) || !is_numeric($hp)) {
    echo "7: uh oh";
    exit();
}

try {
    $sql = 'INSERT INTO `the_builded` SET `type` = :type, `posX` = :posX, `posY` = :posY, `posZ` = :posZ, `rotX` = :rotX, `rotY` = :rotY, `rotZ` = :rotZ, `rotQ` = :rotQ, `hp` = :hp';
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':type', $type, PDO::PARAM_STR);
    $stmt->bindValue(':posX', $posX, PDO::PARAM_STR);
    $stmt->bindValue(':posY', $posY, PDO::PARAM_STR);
    $stmt->bindValue(':posZ', $posZ, PDO::PARAM_STR);
    $stmt->bindValue(':rotX', $rotX, PDO::PARAM_STR);
    $stmt->bindValue(':rotY', $rotY, PDO::PARAM_STR);
    $stmt->bindValue(':rotZ', $rotZ, PDO::PARAM_STR);
    $stmt->bindValue(':rotQ', $rotQ, PDO::PARAM_STR);
    $stmt->bindValue(':hp', $hp, PDO::PARAM_INT);
    $stmt->execute();

    $id = $pdo->lastInsertId();

    echo "0\t".$id;
    exit();
} catch (PDOException $e) {
    echo "Database error: " . $e->getMessage();
    exit();
}

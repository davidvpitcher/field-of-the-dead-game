<?php // additemtoplayer.php

if (isset($_POST["item_name"])) {
    $unity = true;
    $login = true;

    $username = $_POST["username"];
    $itemname = $_POST["item_name"];
    $itemquantity = $_POST["item_quantity"];

    require_once '../../keys/storage.php';

    $sql = "SELECT pi.quantity
            FROM player_items pi
            JOIN players p ON pi.player_id = p.id
            JOIN item i ON pi.item_id = i.id
            WHERE p.username = :username AND i.item_name = :itemname";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username);
    $stmt->bindValue(':itemname', $itemname);
    $stmt->execute();

    $quantityknown = 0;
    $totalfound = 0;

    foreach ($stmt->fetchAll() as $resulto) {
        $totalfound += 1;
        $quantityknown = intval($resulto['quantity']);
    }

    $quantityknown = $itemquantity;

    if ($totalfound == 0) {
        $sql2 = "INSERT INTO `player_items` (player_id, item_id, quantity)
                 VALUES ((SELECT `id` FROM `players` WHERE username = :username), (SELECT `id` FROM `item` WHERE item_name = :itemname), :quantityknown)";
        $stmt2 = $pdo->prepare($sql2);
        $stmt2->bindValue(':username', $username);
        $stmt2->bindValue(':itemname', $itemname);
        $stmt2->bindValue(':quantityknown', $quantityknown);
        $stmt2->execute();
    } else {
        $sql2 = "UPDATE `player_items` SET quantity = :quantityknown
                 WHERE `player_id` = (SELECT `id` FROM `players` WHERE username = :username) AND  `item_id` = (SELECT `id` FROM `item` WHERE item_name = :itemname)";
        $stmt2 = $pdo->prepare($sql2);
        $stmt2->bindValue(':username', $username);
        $stmt2->bindValue(':itemname', $itemname);
        $stmt2->bindValue(':quantityknown', $quantityknown);
        $stmt2->execute();
    }

    echo "0\t" . $totalfound . "\t" . $quantityknown;
    exit();
} else {
    echo "7: uh oh";
    exit();
}
